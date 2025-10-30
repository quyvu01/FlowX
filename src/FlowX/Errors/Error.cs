namespace FlowX.Errors;

public class Error : Exception
{
    public IEnumerable<string> Messages { get; set; }
    public string Code { get; set; }

    public Error(string message) : base(message)
    {
    }

    public Error()
    {
    }
}