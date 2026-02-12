using FlowX.Abstractions;
using FlowX.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Wrappers;

public abstract class RequestHandlerWithResultWrapperBase
{
    public abstract Task<object> HandleAsync(object request, IServiceProvider provider, CancellationToken ct);
}

public abstract class RequestHandlerWrapperBase
{
    public abstract Task HandleAsync(object request, IServiceProvider provider, CancellationToken ct);
    public abstract Task HandleAsync(IRequest request, IServiceProvider provider, CancellationToken ct);
}

public abstract class RequestHandlerWithResultWrapper<TResponse> : RequestHandlerWithResultWrapperBase
{
    public abstract Task<TResponse> HandleAsync(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken ct);
}

public class RequestHandlerWithResultWrapperImpl<TRequest, TResponse> : RequestHandlerWithResultWrapper<TResponse>
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

public class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapperBase where TRequest : IRequest
{
    public override async Task HandleAsync(object request, IServiceProvider provider, CancellationToken ct) =>
        await HandleAsync((IRequest)request, provider, ct).ConfigureAwait(false);

    public override async Task HandleAsync(IRequest request, IServiceProvider provider, CancellationToken ct)
    {
        var flowPipeline = provider.GetRequiredService<FlowPipelinesImpl<TRequest>>();
        await flowPipeline.ExecuteAsync(new FlowContext<TRequest>((TRequest)request, [], ct));
    }
}