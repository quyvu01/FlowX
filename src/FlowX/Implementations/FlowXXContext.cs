using FlowX.Abstractions;

namespace FlowX.Implementations;

public sealed class FlowXXContext<TRequest>(
    TRequest request,
    Dictionary<string, string> headers,
    CancellationToken cancellationToken)
    : IRequestXContext<TRequest>
{
    public Dictionary<string, string> Headers { get; } = headers;
    public CancellationToken CancellationToken { get; } = cancellationToken;
    public TRequest Request { get; } = request;
}