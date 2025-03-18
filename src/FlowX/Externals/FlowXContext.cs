using FlowX.Abstractions;

namespace FlowX.Externals;

public record FlowXContext(Dictionary<string, string> Headers, CancellationToken CancellationToken)
    : IContext;