using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Implementations;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using FlowX.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;

namespace FlowX.Nats.Implementations;

internal class NatsServer<TRequest, TResult>(IServiceProvider serviceProvider)
    : INatsServer<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private readonly ILogger<NatsServer<TRequest, TResult>> _logger =
        serviceProvider.GetService<ILogger<NatsServer<TRequest, TResult>>>();

    private readonly NatsClientWrapper _natsClient = serviceProvider.GetRequiredService<NatsClientWrapper>();

    public async Task SubscribeAsync()
    {
        var natsClient = serviceProvider.GetRequiredService<NatsClientWrapper>();
        var natsScribeAsync = natsClient.NatsClient
            .SubscribeAsync<MessageWrapper>(typeof(TRequest).GetNatsSubject());
        await foreach (var message in natsScribeAsync) _ = ProcessMessageAsync(message);
    }

    private async Task ProcessMessageAsync(NatsMsg<MessageWrapper> message)
    {
        try
        {
            if (message.Data is not { } data) return;
            using var scope = serviceProvider.CreateScope();
            var pipeline = scope.ServiceProvider
                .GetRequiredService<FlowPipelinesImpl<TRequest, TResult>>();
            var request = JsonSerializer.Deserialize<TRequest>(data.MessageJson);
            var headers = message.Headers?
                .ToDictionary(a => a.Key, b => b.Value.ToString()) ?? [];
            var requestContext = new FlowContext<TRequest>(request, headers, CancellationToken.None);
            // Invoke the method and get the result
            try
            {
                var result = await pipeline.ExecuteAsync(requestContext);

                var resultSucceed = Result<TResult>.Success(result);
                await _natsClient.NatsClient.PublishAsync(message.ReplyTo!, resultSucceed);
            }
            catch (Exception e)
            {
                var faultResult = Result<TResult>.Failed(e);
                await _natsClient.NatsClient.PublishAsync(message.ReplyTo!, faultResult);
            }
        }
        catch (Exception e)
        {
            _logger?.LogError("Error while process request: {@Error}", e.Message);
        }
    }
}