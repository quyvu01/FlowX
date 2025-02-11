using FlowX.Abstractions;

namespace FlowX.Implementations;

public class FlowPipelinesImpl<TRequest, TResult>(
    IEnumerable<IFlowPipelineBehavior<TRequest, TResult>> behaviors,
    IRequestHandler<TRequest, TResult> handler)
    where TRequest : IRequest<TResult>
{
    public async Task<TResult> ExecuteAsync(IRequestXContext<TRequest> requestXContext)
    {
        var next = new Func<Task<TResult>>(() => handler.HandleAsync(requestXContext));

        foreach (var behavior in behaviors.Reverse())
        {
            var current = next;
            next = () => behavior.HandleAsync(requestXContext, current);
        }

        return await next();
    }
}