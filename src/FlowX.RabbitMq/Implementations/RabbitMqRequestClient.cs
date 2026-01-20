using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.RabbitMq.Abstractions;
using FlowX.RabbitMq.Constants;
using FlowX.RabbitMq.Extensions;
using FlowX.RabbitMq.Statics;
using FlowX.Responses;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FlowX.RabbitMq.Implementations;

internal class RabbitMqRequestClient : IRequestClient, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<BasicDeliverEventArgs>> _eventArgsMapper = new();
    private IConnection _connection;
    private IChannel _channel;
    private AsyncEventingBasicConsumer _consumer;
    private string _replyQueueName;
    private const string RoutingKey = FlowXRabbitMqConstants.RoutingKey;

    // We have to wait this one and ensure that everything is initialized
    public RabbitMqRequestClient() => StartAsync().Wait();

    private async Task StartAsync()
    {
        var userName = RabbitMqStatics.RabbitMqUserName ?? FlowXRabbitMqConstants.DefaultUserName;
        var password = RabbitMqStatics.RabbitMqPassword ?? FlowXRabbitMqConstants.DefaultPassword;
        var connectionFactory = new ConnectionFactory
        {
            HostName = RabbitMqStatics.RabbitMqHost, VirtualHost = RabbitMqStatics.RabbitVirtualHost,
            Port = RabbitMqStatics.RabbitMqPort, Ssl = RabbitMqStatics.SslOption ?? new SslOption(),
            UserName = userName, Password = password
        };

        _connection = await connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        var queueDeclareResult = await _channel.QueueDeclareAsync();
        _replyQueueName = queueDeclareResult.QueueName;
        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += (_, ea) =>
        {
            var correlationId = ea.BasicProperties.CorrelationId;
            if (string.IsNullOrEmpty(correlationId) || !_eventArgsMapper.TryRemove(correlationId, out var tcs))
                return Task.CompletedTask;
            tcs.TrySetResult(ea);
            return Task.CompletedTask;
        };
        await _channel.BasicConsumeAsync(_replyQueueName, true, _consumer);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.CloseAsync();
        if (_connection is not null) await _connection.CloseAsync();
    }

    public async Task<TResult> RequestAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>
    {
        if (_channel is null) throw new InvalidOperationException();
        var exchangeName = typeof(TRequest).GetExchangeName();
        var cancellationToken = requestContext.CancellationToken;
        var correlationId = Guid.NewGuid().ToString();
        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = _replyQueueName,
            Type = typeof(TRequest).GetAssemblyName()
        };
        props.Headers ??= new Dictionary<string, object>();
        requestContext.Headers?.ForEach(h => props.Headers.Add(h.Key, h.Value));

        var tcs = new TaskCompletionSource<BasicDeliverEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        _eventArgsMapper.TryAdd(correlationId, tcs);
        var messageSerialize = JsonSerializer.Serialize(requestContext.Request);
        var messageBytes = Encoding.UTF8.GetBytes(messageSerialize);
        await _channel.BasicPublishAsync(exchangeName, routingKey: RoutingKey,
            mandatory: true, basicProperties: props, body: messageBytes, cancellationToken: cancellationToken);

        await using var ctr = cancellationToken.Register(() =>
        {
            _eventArgsMapper.TryRemove(correlationId, out _);
            tcs.SetCanceled(cancellationToken);
        });
        var result = await tcs.Task;
        var resultWrapped = JsonSerializer.Deserialize<Result<TResult>>(Encoding.UTF8.GetString(result.Body.ToArray()));
        return resultWrapped.IsSuccess ? resultWrapped.Data : throw resultWrapped.Fault.ToException();
    }
}