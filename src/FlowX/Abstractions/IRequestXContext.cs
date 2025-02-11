namespace FlowX.Abstractions;

public interface IRequestXContext<out TRequest> : Context
{
    TRequest Request { get; }
}