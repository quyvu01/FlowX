using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Cached;
using FlowX.RabbitMq.Abstractions;
using FlowX.RabbitMq.Constants;
using FlowX.RabbitMq.Extensions;
using FlowX.RabbitMq.Internal;
using FlowX.RabbitMq.Statics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FlowX.RabbitMq.Implementations;

internal class RabbitMqServer(IServiceProvider serviceProvider) : IRabbitMqServer
{
    private static readonly Lazy<ConcurrentDictionary<string, Type>> RequestAssemblyCached = new(() => []);

    private IConnection _connection;
    private IChannel _channel;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var queueName = $"{FlowXRabbitMqConstants.QueueNamePrefix}-{AppDomain.CurrentDomain.FriendlyName.ToLower()}";
        const string routingKey = FlowXRabbitMqConstants.RoutingKey;

        var userName = RabbitMqStatics.RabbitMqUserName ?? FlowXRabbitMqConstants.DefaultUserName;
        var password = RabbitMqStatics.RabbitMqPassword ?? FlowXRabbitMqConstants.DefaultPassword;
        var connectionFactory = new ConnectionFactory
        {
            HostName = RabbitMqStatics.RabbitMqHost, VirtualHost = RabbitMqStatics.RabbitVirtualHost,
            Port = RabbitMqStatics.RabbitMqPort, Ssl = RabbitMqStatics.SslOption ?? new SslOption(),
            UserName = userName, Password = password
        };

        _connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false,
            autoDelete: false, arguments: null, cancellationToken: cancellationToken);

        var requestMapResponseTypes = FlowXCached.RequestMapResponse;

        foreach (var exchangeName in requestMapResponseTypes.Select(kv => kv.Key.GetExchangeName()))
        {
            await _channel.ExchangeDeclareAsync(exchangeName, type: ExchangeType.Direct,
                cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: queueName, exchangeName, routingKey,
                cancellationToken: cancellationToken);
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var cons = (AsyncEventingBasicConsumer)sender;
            var ch = cons.Channel;
            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = new BasicProperties { CorrelationId = props.CorrelationId };

            var requestType = RequestAssemblyCached.Value.GetOrAdd(props.Type, static type => Type.GetType(type)!);
            var responseType = requestMapResponseTypes[requestType];
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var headerInjector = scope.ServiceProvider.GetRequiredService<IInternalHeaderInjector>();
            try
            {
                var message = JsonSerializer.Deserialize(Encoding.UTF8.GetString(body), requestType);
                var headers = props.Headers?
                    .ToDictionary(a => a.Key, b => b.Value.ToString()) ?? [];
                headerInjector.Headers = headers;
                var response = await mediator.Send(message, ea.CancellationToken);
                var result = ResultWrapped.NewResultWrapper(responseType, response, null);
                var responseAsString = JsonSerializer.Serialize(result);
                var responseBytes = Encoding.UTF8.GetBytes(responseAsString);
                await ch.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!,
                    mandatory: true, basicProperties: replyProps, body: responseBytes,
                    cancellationToken: ea.CancellationToken);
            }
            catch (Exception e)
            {
                var logger = serviceProvider.GetService<ILogger<RabbitMqServer>>();
                logger.LogError("Error while responding <{@Attribute}> with message : {@Error}", props.Type, e);
                var result = ResultWrapped.NewResultWrapper(responseType, null, e);
                var responseAsString = JsonSerializer.Serialize(result);
                var responseBytes = Encoding.UTF8.GetBytes(responseAsString);
                await ch.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!,
                    mandatory: true, basicProperties: replyProps, body: responseBytes,
                    cancellationToken: ea.CancellationToken);
            }
            finally
            {
                await ch.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false,
                    cancellationToken: ea.CancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _channel.Dispose();
        _connection.Dispose();
        return Task.CompletedTask;
    }
}