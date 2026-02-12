using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;

namespace FlowX.Nats.Implementations;

public sealed class NatsTransportService(IServiceProvider serviceProvider) : ITransportService
{
    public Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>
    {
        var natsRequester = serviceProvider.GetService<INatsClient<TRequest, TResult>>();
        return natsRequester.RequestAsync(requestContext);
    }

    public async Task TransportDataAsync<TRequest>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest
    {
        var natsRequester = serviceProvider.GetService<INatsClient<TRequest>>();
        await natsRequester.RequestAsync(requestContext);
    }
}