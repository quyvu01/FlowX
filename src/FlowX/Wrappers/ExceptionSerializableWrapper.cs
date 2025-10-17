using FlowX.Errors;

namespace FlowX.Wrappers;

public sealed class ExceptionSerializableWrapper
{
    public string TypeAssembly { get; set; }
    public string Message { get; set; }
    public IEnumerable<string> Messages { get; set; }
    public string Code { get; set; }
    public string StackTrace { get; set; }

    public static ExceptionSerializableWrapper FromException(Exception e) => e switch
    {
        Error error => new ExceptionSerializableWrapper
            { TypeAssembly = typeof(Error).AssemblyQualifiedName, Code = error.Code, Messages = error.Messages },
        _ => new ExceptionSerializableWrapper
        {
            TypeAssembly = e.GetType().AssemblyQualifiedName, Message = e.Message, StackTrace = e.StackTrace
        }
    };

    public static Exception ToException(ExceptionSerializableWrapper e) =>
        e.TypeAssembly == typeof(Error).AssemblyQualifiedName
            ? new Error { Code = e.Code, Messages = e.Messages }
            : new Exception(e.Message, new Exception(e.StackTrace));
}