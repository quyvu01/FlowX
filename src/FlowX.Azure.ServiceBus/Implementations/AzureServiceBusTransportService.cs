using FlowX.Abstractions;
using FlowX.Azure.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Azure.ServiceBus.Implementations;

public sealed class AzureServiceBusTransportService(IServiceProvider serviceProvider) : ITransportService
{
    public Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>
    {
        var natsRequester = serviceProvider.GetService<IAzureServiceBusClient<TRequest, TResult>>();
        return natsRequester.RequestAsync(requestContext);
    }
}