namespace FlowX.Abstractions;

public interface ITransportService
{
    Task<TResult> TransportDataAsync<TRequest, TResult>(IRequestXContext<TRequest> requestXContext)
        where TRequest : IRequest<TResult>;
}