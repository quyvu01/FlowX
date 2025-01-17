namespace FlowX.Abstractions;

public interface ITransportService
{
    Task<TResult> TransportDataAsync<TRequest, TResult>(RequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>;
}