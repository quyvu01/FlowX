namespace FlowX.Abstractions;

public interface IRequestBase;
public interface IRequest<out TResult> : IRequestBase;