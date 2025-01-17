using FlowX.Abstractions;

namespace FlowX.Externals;

public record ExternalRequestContext(Dictionary<string, string> Headers, CancellationToken CancellationToken)
    : FlowContext;