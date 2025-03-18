namespace FlowX.Abstractions;

public interface IContext
{
    Dictionary<string, string> Headers { get; }
    CancellationToken CancellationToken { get; }
}