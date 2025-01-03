namespace FlowX.Errors;

public sealed class Error : Exception
{
    public IEnumerable<string> Messages { get; set; }
    public string Code { get; set; }
}