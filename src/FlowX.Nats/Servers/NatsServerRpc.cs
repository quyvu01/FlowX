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
using NATS.Client.Core;

namespace FlowX.Nats.Servers;

internal class NatsServerRpc<TRequest, TResult>(IServiceProvider serviceProvider)
    : INatsServerRpc<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private readonly ILogger<NatsServerRpc<TRequest, TResult>> _logger =
        serviceProvider.GetService<ILogger<NatsServerRpc<TRequest, TResult>>>();

    public async Task SubscribeAsync()
    {
        var natsClient = serviceProvider.GetRequiredService<NatsClientWrapper>();
        var natsScribeAsync = natsClient.NatsClient
            .SubscribeAsync<NatsMessageWrapper>(typeof(TRequest).GetNatsSubject());
        await foreach (var message in natsScribeAsync)
        {
            var replyAddress = message.ReplyTo!;
            _ = ProcessMessageAsync(message, natsClient, replyAddress);
        }
    }

    private async Task ProcessMessageAsync(NatsMsg<NatsMessageWrapper> message, NatsClientWrapper natsClient,
        string replyAddress)
    {
        try
        {
            if (message.Data is not { } data) return;
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
                return;
            }

            var messageSerialize = new MessageSerialized
                { Type = typeof(TResult).GetAssemblyName(), ObjectSerialized = JsonSerializer.Serialize(response) };
            await natsClient.NatsClient.PublishAsync(replyAddress, messageSerialize);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while process request: {@Error}", e.Message);
        }
    }
}