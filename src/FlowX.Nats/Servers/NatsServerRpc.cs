using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Implementations;
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

    private readonly NatsClientWrapper _natsClient = serviceProvider.GetRequiredService<NatsClientWrapper>();

    public async Task SubscribeAsync()
    {
        var natsClient = serviceProvider.GetRequiredService<NatsClientWrapper>();
        var natsScribeAsync = natsClient.NatsClient
            .SubscribeAsync<NatsMessageWrapper>(typeof(TRequest).GetNatsSubject());
        await foreach (var message in natsScribeAsync) _ = ProcessMessageAsync(message);
    }

    private async Task ProcessMessageAsync(NatsMsg<NatsMessageWrapper> message)
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
            await _natsClient.NatsClient.PublishAsync(message.ReplyTo!, response);
        }
        catch (Exception e)
        {
            _logger?.LogError("Error while process request: {@Error}", e.Message);
        }
    }
}