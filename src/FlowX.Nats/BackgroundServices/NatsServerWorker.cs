using FlowX.Cached;
using FlowX.Nats.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlowX.Nats.BackgroundServices;

internal sealed class NatsServerWorker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var requestMapResponseTypes = FlowXCached.RequestMapResponse;
        var tasks = requestMapResponseTypes.Select(async requestMapResponseType =>
        {
            var (requestType, responseType) = requestMapResponseType;
            var service = serviceProvider.GetRequiredService(typeof(INatsServerRpc<,>)
                .MakeGenericType(requestType, responseType));
            if (service is not INatsServerRpc natsServerRpc) return;
            await natsServerRpc.SubscribeAsync();
        });
        await Task.WhenAll(tasks);
    }
}