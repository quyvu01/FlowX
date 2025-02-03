using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Implementations;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Nats.Servers;

internal class NatsServerRpc<TRequest, TResult>(IServiceProvider serviceProvider)
    : INatsServerRpc<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task SubscribeAsync()
    {
        var natsClient = serviceProvider.GetRequiredService<NatsClientWrapper>();
        var natsScribeAsync = natsClient.NatsClient
            .SubscribeAsync<NatMessageWrapper>(typeof(TRequest).GetNatsSubject());
        await foreach (var message in natsScribeAsync)
        {
            if (message.Data is not { } data) continue;
            var pipeline = serviceProvider
                .GetRequiredService<FlowPipelinesImpl<TRequest, TResult>>();
            var request = JsonSerializer.Deserialize<TRequest>(data.MessageAsString);
            var headers = message.Headers?
                .ToDictionary(a => a.Key, b => b.Value.ToString()) ?? [];
            var requestContext = new FlowXContext<TRequest>(request, headers, CancellationToken.None);
            // Invoke the method and get the result
            var response = await pipeline.ExecuteAsync(requestContext);
            await natsClient.NatsClient.PublishAsync(message.ReplyTo!, response);
        }
    }
}