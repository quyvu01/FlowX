namespace FlowX.Abstractions;

public interface IFlowPipelineBehavior<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(IRequestXContext<TRequest> requestXContext, Func<Task<TResult>> next);
}