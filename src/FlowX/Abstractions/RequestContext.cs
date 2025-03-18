namespace FlowX.Abstractions;

public interface IRequestContext<out TRequest> : IContext
{
    TRequest Request { get; }
}