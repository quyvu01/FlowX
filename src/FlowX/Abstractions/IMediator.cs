namespace FlowX.Abstractions;

public interface IMediator
{
    Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
    Task<object> Send(object request, CancellationToken cancellationToken = default);
}