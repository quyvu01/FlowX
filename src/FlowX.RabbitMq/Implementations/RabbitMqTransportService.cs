using FlowX.Abstractions;
using FlowX.RabbitMq.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.RabbitMq.Implementations;

public sealed class RabbitMqTransportService(IServiceProvider serviceProvider) : ITransportService
{
    public async Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>
    {
        var requestClient = serviceProvider.GetService<IRequestClient>();
        var result = await requestClient.RequestAsync<TRequest, TResult>(requestContext);
        return result;
    }
}