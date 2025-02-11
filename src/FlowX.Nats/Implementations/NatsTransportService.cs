using FlowX.Abstractions;
using FlowX.Nats.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Nats.Implementations;

public sealed class NatsTransportService(IServiceProvider serviceProvider) : ITransportService
{
    public Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestXContext<TRequest> requestXContext)
        where TRequest : IRequest<TResult>
    {
        var natsRequester = serviceProvider.GetService<INatsRequester<TRequest, TResult>>();
        return natsRequester.RequestAsync(requestXContext);
    }
}