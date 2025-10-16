using FlowX.Abstractions;
using FlowX.Nats.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Nats.Implementations;

public sealed class NatsTransportService(IServiceProvider serviceProvider) : ITransportService
{
    public Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>
    {
        var natsRequester = serviceProvider.GetService<INatsClient<TRequest, TResult>>();
        return natsRequester.RequestAsync(requestContext);
    }
}