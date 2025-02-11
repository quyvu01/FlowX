namespace FlowX.Abstractions;

public interface IFlowXSender
{
    Task<TResult> Send<TResult>(IRequest<TResult> requestContext, Context context = null);
}