namespace FlowX.RabbitMq.Abstractions;

internal interface IInternalHeaderInjector
{
    public Dictionary<string, string> Headers { get; set; }
}