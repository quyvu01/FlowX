using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.RabbitMq.Abstractions;

namespace FlowX.RabbitMq.InternalPipelines;

internal sealed class InternalHeadersPipelineBehavior<TRequest, TResult>(IInternalHeaderInjector headerInjector)
    : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public async Task<TResult> HandleAsync(IRequestContext<TRequest> requestContext, Func<Task<TResult>> next)
    {
        headerInjector.Headers?.ForEach(a => requestContext.Headers.Add(a.Key, a.Value));
        var result = await next.Invoke();
        return result;
    }
}