using FlowX.RabbitMq.Abstractions;

namespace FlowX.RabbitMq.Implementations;

public sealed class InternalHeaderInjector : IInternalHeaderInjector
{
    public Dictionary<string, string> Headers { get; set; }
}