namespace FlowX.Abstractions;

public interface ITransportService
{
    Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>;
}