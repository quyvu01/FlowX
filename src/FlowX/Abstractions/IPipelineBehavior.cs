namespace FlowX.Abstractions;

public interface IPipelineBehavior<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(IRequestContext<TRequest> requestContext, Func<Task<TResult>> next);
}