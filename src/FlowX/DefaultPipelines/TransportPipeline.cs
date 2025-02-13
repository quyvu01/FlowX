using FlowX.Abstractions;
using FlowX.Cached;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.DefaultPipelines;

/// <summary>
/// The `TransportPipelineImpl` is the default pipeline of FlowX, which automatically handles requests across services.
/// First, it checks if the application has a handler registered for the request. If a handler is found, the request is processed further.
/// Otherwise, it checks for any supported transports and forwards the request to them if available.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResult"></typeparam>
internal sealed class TransportPipeline<TRequest, TResult>(IServiceProvider serviceProvider)
    : IFlowPipelineBehavior<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task<TResult> HandleAsync(RequestContext<TRequest> requestXContext, Func<Task<TResult>> next)
    {
        var isRequestRegisteredInApp = FlowXCached.RequestMapResponse
            .TryGetValue(typeof(TRequest), out _);
        if (isRequestRegisteredInApp) return await next.Invoke();
        // Check if transport is supported or not!
        var transportService = serviceProvider.GetService<ITransportService>();
        if (transportService is null) return await next.Invoke();
        return await transportService.TransportDataAsync<TRequest, TResult>(requestXContext);
    }
}