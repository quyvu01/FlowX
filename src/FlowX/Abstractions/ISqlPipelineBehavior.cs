namespace FlowX.Abstractions;

public interface ISqlPipelineBehavior<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(RequestContext<TRequest> requestContext,
        Func<Task<TResult>> next);
}