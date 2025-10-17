using FlowX.Wrappers;

namespace FlowX.Nats.Wrappers;

public sealed class NatResponseWrapped<TResponse>
{
    public string TypeAssembly { get; set; }
    public TResponse Response { get; set; }
    public ExceptionSerializableWrapper ExceptionSerializable { get; set; }
}