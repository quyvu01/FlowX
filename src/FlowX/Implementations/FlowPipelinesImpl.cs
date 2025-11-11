using FlowX.Abstractions;

namespace FlowX.Implementations;

public class FlowPipelinesImpl<TRequest, TResult>(
    IEnumerable<IPipelineBehavior<TRequest, TResult>> behaviors,
    IRequestHandler<TRequest, TResult> handler)
    where TRequest : IRequest<TResult>
{
    public Task<TResult> ExecuteAsync(IRequestContext<TRequest> requestContext) =>
        behaviors.Reverse().Aggregate(() => handler.HandleAsync(requestContext),
            (acc, pipeline) => () => pipeline.HandleAsync(requestContext, acc)).Invoke();
}