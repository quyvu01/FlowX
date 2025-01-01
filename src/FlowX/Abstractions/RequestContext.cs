namespace FlowX.Abstractions;

public interface FlowContext
{
    Dictionary<string, string> Headers { get; }
    CancellationToken CancellationToken { get; }
}

public interface RequestContext<TRequest> : FlowContext
{
    TRequest Request { get; }
}