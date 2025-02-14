namespace FlowX.Abstractions;

public interface IFlow
{
    Task<TResult> Send<TResult>(IRequest<TResult> requestContext, Context context = null);
    Task<object> Send(object request, Context context = null);
}