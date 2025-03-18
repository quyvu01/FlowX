namespace FlowX.Abstractions;

public interface IFlow
{
    Task<TResult> Send<TResult>(IRequest<TResult> requestContext, IContext context = null);
    Task<object> Send(object request, IContext context = null);
}