namespace FlowX.Abstractions;

public interface Context
{
    Dictionary<string, string> Headers { get; }
    CancellationToken CancellationToken { get; }
}