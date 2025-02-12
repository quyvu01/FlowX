namespace FlowX.Abstractions;

public interface IFlow
{
    Task<TResult> Send<TResult>(IRequest<TResult> requestContext, Context context = null);
}