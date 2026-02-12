namespace FlowX.Abstractions;

public interface IRequestHandlerBase;

public interface IRequestHandler<in TRequest, TResult> : IRequestHandlerBase where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(IRequestContext<TRequest> requestContext);
}

public interface IRequestHandler<in TRequest> : IRequestHandlerBase where TRequest : IRequest
{
    Task HandleAsync(IRequestContext<TRequest> requestContext);
}