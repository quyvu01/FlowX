using Azure.Messaging.ServiceBus.Administration;
using FlowX.Azure.ServiceBus.Abstractions;
using FlowX.Azure.ServiceBus.Extensions;
using FlowX.Azure.ServiceBus.Wrappers;
using FlowX.Cached;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowX.Azure.ServiceBus.BackgroundServices;

internal sealed class AzureServiceBusServerWorker(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<AzureServiceBusServerWorker> _logger =
        serviceProvider.GetService<ILogger<AzureServiceBusServerWorker>>();

    private readonly AzureServiceBusClientWrapper _clientWrapper =
        serviceProvider.GetService<AzureServiceBusClientWrapper>();

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
                    await CreateQueueIfNotExistedAsync(requestType.GetAzureServiceBusRequestQueue(), stoppingToken);
                    await CreateQueueIfNotExistedAsync(responseType.GetAzureServiceBusReplyQueue(), stoppingToken);
                    var service = serviceProvider.GetRequiredService(typeof(IAzureServiceBusServer<,>)
                        .MakeGenericType(requestType, responseType));
                    if (service is not IAzureServiceBusServer serviceBusServer) return;
                    await serviceBusServer.StartAsync();
                });
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger?.LogError("Error while starting Azure Service Bus: {@Message}", e.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task CreateQueueIfNotExistedAsync(string queueName, CancellationToken cancellationToken)
    {
        var adminClient = _clientWrapper.ServiceBusAdministrationClient;
        var queueExisted = await adminClient.QueueExistsAsync(queueName, cancellationToken);
        if (!queueExisted)
            await adminClient.CreateQueueAsync(new CreateQueueOptions(queueName) { RequiresSession = true },
                cancellationToken);
    }
}