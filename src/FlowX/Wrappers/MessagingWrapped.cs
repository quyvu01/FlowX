namespace FlowX.Wrappers;

public abstract class MessagingWrapped
{
    public abstract void SetResponse(object response);
    public abstract void SetError(Exception e);
}

public sealed class MessagingWrapped<TResponse> : MessagingWrapped
{
    public string TypeAssembly { get; set; }
    public TResponse Response { get; set; }
    public ExceptionSerializableWrapper ExceptionSerializable { get; set; }

    public override void SetResponse(object response)
    {
        if (response is not TResponse rs) return;
        Response = rs;
    }

    public override void SetError(Exception e)
    {
        if (e is null) return;
        TypeAssembly = e.GetType().AssemblyQualifiedName;
        ExceptionSerializable = ExceptionSerializableWrapper.FromException(e);
    }
}