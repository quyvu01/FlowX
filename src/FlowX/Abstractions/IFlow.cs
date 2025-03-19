namespace FlowX.Abstractions;

public interface IFlow
{
    Task<TResult> Send<TResult>(IRequest<TResult> requestContext, CancellationToken cancellationToken = default);
    Task<object> Send(object request, CancellationToken cancellationToken = default);
}