namespace FlowX.Abstractions;

public interface IRequestHandler<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(RequestContext<TRequest> requestContext);
}