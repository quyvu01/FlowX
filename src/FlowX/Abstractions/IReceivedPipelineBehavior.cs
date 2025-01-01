namespace FlowX.Abstractions;

public interface IReceivedPipelineBehavior<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(RequestContext<TRequest> requestContext,
        Func<Task<TResult>> next);
}