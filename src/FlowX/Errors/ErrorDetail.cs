namespace FlowX.Errors;

public sealed class ErrorDetail : Exception
{
    public IEnumerable<string> Messages { get; set; }
    public string Code { get; set; }
}