using FlowX.Cached;
using FlowX.Nats.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowX.Nats.BackgroundServices;

internal sealed class NatsServerWorker(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<NatsServerWorker> _logger = serviceProvider.GetService<ILogger<NatsServerWorker>>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
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
            catch (Exception e)
            {
                _logger?.LogError("Error while starting Nats: {@Message}", e.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}