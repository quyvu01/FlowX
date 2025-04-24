using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.Implementations;
using FlowX.Messages;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlowX.Nats.Servers;

internal class NatsServerRpc<TRequest, TResult>(IServiceProvider serviceProvider)
    : INatsServerRpc<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task SubscribeAsync()
    {
        var natsClient = serviceProvider.GetRequiredService<NatsClientWrapper>();
        var natsScribeAsync = natsClient.NatsClient
            .SubscribeAsync<NatsMessageWrapper>(typeof(TRequest).GetNatsSubject());
        var logger = serviceProvider.GetRequiredService<ILogger<NatsServerRpc<TRequest, TResult>>>();
        await foreach (var message in natsScribeAsync)
        {
            var replyAddress = message.ReplyTo!;
            try
            {
                if (message.Data is not { } data) continue;
                using var scope = serviceProvider.CreateScope();
                var pipeline = scope.ServiceProvider
                    .GetRequiredService<FlowPipelinesImpl<TRequest, TResult>>();
                var request = JsonSerializer.Deserialize<TRequest>(data.MessageAsString);
                var headers = message.Headers?
                    .ToDictionary(a => a.Key, b => b.Value.ToString()) ?? [];
                var requestContext = new FlowContext<TRequest>(request, headers, CancellationToken.None);
                // Invoke the method and get the result
                var response = await pipeline.ExecuteAsync(requestContext);
                if (response is IMessageSerialized oneOf)
                {
                    await natsClient.NatsClient.PublishAsync(replyAddress, oneOf.Serialize());
                    continue;
                }

                var messageSerialize = new MessageSerialized
                    { Type = typeof(TResult).GetAssemblyName(), ObjectSerialized = JsonSerializer.Serialize(response) };
                await natsClient.NatsClient.PublishAsync(replyAddress, messageSerialize);
            }
            catch (Exception e)
            {
                logger.LogError("Error while process request: {@Error}", e.Message);
            }
        }
    }
}