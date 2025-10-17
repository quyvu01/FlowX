using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using FlowX.Abstractions;
using FlowX.Azure.ServiceBus.Abstractions;
using FlowX.Azure.ServiceBus.Extensions;
using FlowX.Azure.ServiceBus.Statics;
using FlowX.Azure.ServiceBus.Wrappers;
using FlowX.Extensions;
using FlowX.Statics;
using FlowX.Wrappers;

namespace FlowX.Azure.ServiceBus.Implementations;

internal sealed class AzureServiceBusClient<TRequest, TResult>
    : IAzureServiceBusClient<TRequest, TResult>, IAsyncDisposable where TRequest : IRequest<TResult>
{
    private readonly ServiceBusSender _serviceBusSender;
    private readonly ServiceBusSessionProcessor _replyProcessor;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<BinaryData>> _pendingReplies = new();
    private readonly string _sessionId;
    private readonly string _replyQueueName;

    public AzureServiceBusClient(AzureServiceBusClientWrapper clientWrapper)
    {
        var client = clientWrapper.ServiceBusClient;
        _sessionId = Guid.NewGuid().ToString();
        var requestQueueName = typeof(TRequest).GetAzureServiceBusRequestQueue();
        _replyQueueName = typeof(TResult).GetAzureServiceBusReplyQueue();
        _serviceBusSender = client.CreateSender(requestQueueName);
        _replyProcessor = client.CreateSessionProcessor(_replyQueueName, new ServiceBusSessionProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentSessions = AzureServiceBusStatic.MaxConcurrentSessions,
            MaxConcurrentCallsPerSession = 1,
            SessionIds = { _sessionId }
        });

        _replyProcessor.ProcessMessageAsync += ProcessReplyAsync;
        _replyProcessor.ProcessErrorAsync += _ => Task.CompletedTask;
        _replyProcessor.StartProcessingAsync().Wait();
    }

    public async Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext)
    {
        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<BinaryData>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingReplies[correlationId] = tcs;
        var messageWrapped = new MessageWrapper
            { MessageJson = JsonSerializer.Serialize(requestContext.Request) };

        var messageSerialize = JsonSerializer.Serialize(messageWrapped);
        var requestMessage = new ServiceBusMessage(messageSerialize)
        {
            CorrelationId = correlationId,
            ReplyTo = _replyQueueName,
            SessionId = _sessionId
        };
        requestContext.Headers?.ForEach(h => requestMessage.ApplicationProperties.Add(h.Key, h.Value));
        await _serviceBusSender.SendMessageAsync(requestMessage, requestContext.CancellationToken);
        var taskAny = await Task.WhenAny(tcs.Task, Task.Delay(FlowXStatics.DefaultRequestTimeout));
        if (taskAny != tcs.Task)
        {
            var exception = new TimeoutException($"Timeout waiting for {nameof(ServiceBusMessage)} to complete!");
            tcs.TrySetException(exception);
            throw exception;
        }

        var result = await tcs.Task;
        var resultWrapped = result.ToObjectFromJson<MessagingResponseWrapped<TResult>>();
        return resultWrapped.TypeAssembly is null
            ? resultWrapped.Response
            : throw ExceptionSerializableWrapper.ToException(resultWrapped.ExceptionSerializable);
    }

    private async Task ProcessReplyAsync(ProcessSessionMessageEventArgs args)
    {
        var msg = args.Message;
        if (_pendingReplies.TryRemove(msg.CorrelationId, out var tcs)) tcs.TrySetResult(msg.Body);
        await args.CompleteMessageAsync(msg);
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceBusSender != null) await _serviceBusSender.DisposeAsync();
        if (_replyProcessor != null) await _replyProcessor.DisposeAsync();
    }
}