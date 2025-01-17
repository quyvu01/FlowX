using FlowX.Abstractions;

namespace FlowX.Implementations;

internal class FlowPipelinesImpl<TRequest, TResult>(
    IEnumerable<IFlowPipelineBehavior<TRequest, TResult>> behaviors,
    IRequestHandler<TRequest, TResult> handler)
    where TRequest : IRequest<TResult>
{
    public async Task<TResult> ExecuteAsync(RequestContext<TRequest> requestContext)
    {
        var next = new Func<Task<TResult>>(() => handler.HandleAsync(requestContext));

        foreach (var behavior in behaviors.Reverse())
        {
            var current = next;
            next = () => behavior.HandleAsync(requestContext, current);
        }

        return await next();
    }
}