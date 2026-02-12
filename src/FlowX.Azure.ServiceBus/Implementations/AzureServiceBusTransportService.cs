using FlowX.Abstractions;
using FlowX.Azure.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Azure.ServiceBus.Implementations;

public sealed class AzureServiceBusTransportService(IServiceProvider serviceProvider) : ITransportService
{
    public Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>
    {
        var client = serviceProvider.GetService<IAzureServiceBusClient<TRequest, TResult>>();
        return client.RequestAsync(requestContext);
    }

    public async Task TransportDataAsync<TRequest>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest
    {
        var client = serviceProvider.GetService<IAzureServiceBusClient<TRequest>>();
        await client.RequestAsync(requestContext);
    }
}