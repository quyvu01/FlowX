namespace FlowX.Wrappers;

public sealed class MessagingResponseWrapped<TResponse>
{
    public string TypeAssembly { get; set; }
    public TResponse Response { get; set; }
    public ExceptionSerializableWrapper ExceptionSerializable { get; set; }
}