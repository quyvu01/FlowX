namespace FlowX.Abstractions;

public interface FlowContext
{
    Dictionary<string, string> Headers { get; }
    CancellationToken CancellationToken { get; }
}