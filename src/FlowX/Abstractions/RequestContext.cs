namespace FlowX.Abstractions;

public interface RequestContext<out TRequest> : Context
{
    TRequest Request { get; }
}