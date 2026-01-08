using FlowX.RabbitMq.Abstractions;
using Microsoft.Extensions.Hosting;

namespace FlowX.RabbitMq.BackgroundServices;

internal sealed class RabbitMqServerHostedService(IRabbitMqServer rabbitMqServer) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await rabbitMqServer.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await rabbitMqServer.StopAsync(cancellationToken);
    }
}