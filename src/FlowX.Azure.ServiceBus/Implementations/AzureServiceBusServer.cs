using System.Text.Json;
using Azure.Messaging.ServiceBus;
using FlowX.Abstractions;
using FlowX.Azure.ServiceBus.Abstractions;
using FlowX.Azure.ServiceBus.Extensions;
using FlowX.Azure.ServiceBus.Statics;
using FlowX.Azure.ServiceBus.Wrappers;
using FlowX.Implementations;
using FlowX.Statics;
using FlowX.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlowX.Azure.ServiceBus.Implementations;

internal class AzureServiceBusServer<TRequest, TResult>(
    AzureServiceBusClientWrapper clientWrapper,
    IServiceProvider serviceProvider)
    : IAzureServiceBusServer<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private readonly ILogger<AzureServiceBusServer<TRequest, TResult>> _logger =
        serviceProvider.GetService<ILogger<AzureServiceBusServer<TRequest, TResult>>>();


    public async Task StartAsync()
    {
        var requestQueue = typeof(TRequest).GetAzureServiceBusRequestQueue();
        var options = new ServiceBusSessionProcessorOptions
        {
            MaxConcurrentSessions = AzureServiceBusStatic.MaxConcurrentSessions,
            MaxConcurrentCallsPerSession = 1,
            AutoCompleteMessages = false
        };
        var processor = clientWrapper.ServiceBusClient.CreateSessionProcessor(requestQueue, options);

        processor.ProcessMessageAsync += async args =>
        {
            var message = args.Message;
            using var scope = serviceProvider.CreateScope();
            var pipeline = scope.ServiceProvider
                .GetRequiredService<FlowPipelinesImpl<TRequest, TResult>>();
            var messageWrapper = JsonSerializer.Deserialize<MessageWrapper>(message.Body);

            var headers = message.ApplicationProperties?
                .ToDictionary(a => a.Key, b => b.Value.ToString()) ?? [];
            var request = JsonSerializer.Deserialize<TRequest>(messageWrapper.MessageJson);
            
            var requestContext = new FlowContext<TRequest>(request, headers, CancellationToken.None);
            var sender = clientWrapper.ServiceBusClient.CreateSender(message.ReplyTo);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
            cts.CancelAfter(FlowXStatics.DefaultRequestTimeout);

            try
            {
                var result = await pipeline.ExecuteAsync(requestContext);
                var response = new MessagingResponseWrapped<TResult> { Response = result };
                var responseMessage = new ServiceBusMessage(JsonSerializer.Serialize(response))
                {
                    CorrelationId = message.CorrelationId,
                    SessionId = message.SessionId
                };

                await sender.SendMessageAsync(responseMessage, cts.Token);
            }
            catch (Exception e)
            {
                var exceptionAsResponse = new MessagingResponseWrapped<TResult>
                {
                    ExceptionSerializable = ExceptionSerializableWrapper.FromException(e),
                    TypeAssembly = e.GetType().AssemblyQualifiedName
                };
                var responseMessage = new ServiceBusMessage(JsonSerializer.Serialize(exceptionAsResponse))
                {
                    CorrelationId = message.CorrelationId,
                    SessionId = message.SessionId
                };
                await sender.SendMessageAsync(responseMessage, cts.Token);
            }
            finally
            {
                await args.CompleteMessageAsync(message, cts.Token);
            }
        };
        processor.ProcessErrorAsync += args =>
        {
            _logger?.LogError("Error while process request: {@Error}", args.Exception.Message);
            return Task.CompletedTask;
        };
        await processor.StartProcessingAsync();
        await new TaskCompletionSource().Task;
    }
}