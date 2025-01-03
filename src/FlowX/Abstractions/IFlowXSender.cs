namespace FlowX.Abstractions;

public interface IFlowXSender
{
    Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> requestContext, FlowContext context = null);
}