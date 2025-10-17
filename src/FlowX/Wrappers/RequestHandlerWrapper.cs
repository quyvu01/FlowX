using FlowX.Abstractions;
using FlowX.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Wrappers;

public abstract class RequestHandlerWrapperBase
{
    public abstract Task<object> HandleAsync(object request, IServiceProvider provider, CancellationToken ct);
}

public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerWrapperBase
{
    public abstract Task<TResponse> HandleAsync(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken ct);
}

public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override async Task<TResponse> HandleAsync(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var flowPipeline = serviceProvider.GetRequiredService<FlowPipelinesImpl<TRequest, TResponse>>();
        return await flowPipeline.ExecuteAsync(new FlowContext<TRequest>((TRequest)request, [], ct));
    }

    public override async Task<object> HandleAsync(object request, IServiceProvider serviceProvider,
        CancellationToken ct) =>
        await HandleAsync((IRequest<TResponse>)request, serviceProvider, ct).ConfigureAwait(false);
}