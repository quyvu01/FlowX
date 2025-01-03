namespace FlowX.Abstractions;

public interface IRequestHandlerBase;

public interface IRequestHandler<in TRequest, TResult> : IRequestHandlerBase where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(RequestContext<TRequest> requestContext);
}