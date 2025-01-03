using FlowX.Abstractions;

namespace FlowX.Externals;

public class ExternalRequestContext(Dictionary<string, string> headers, CancellationToken cancellationToken)
    : FlowContext
{
    public Dictionary<string, string> Headers { get; } = headers;
    public CancellationToken CancellationToken { get; } = cancellationToken;
}