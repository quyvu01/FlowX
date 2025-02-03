using FlowX.Abstractions;

namespace FlowX.Implementations;

public sealed class FlowXContext<TRequest>(
    TRequest request,
    Dictionary<string, string> headers,
    CancellationToken cancellationToken)
    : RequestContext<TRequest>
{
    public Dictionary<string, string> Headers { get; } = headers;
    public CancellationToken CancellationToken { get; } = cancellationToken;
    public TRequest Request { get; } = request;
}