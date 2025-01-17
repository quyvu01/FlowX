namespace FlowX.Abstractions;

public interface RequestContext<out TRequest> : FlowContext
{
    TRequest Request { get; }
}