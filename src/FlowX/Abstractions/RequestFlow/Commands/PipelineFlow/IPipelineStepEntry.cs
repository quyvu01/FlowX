namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public interface IPipelineStepEntry
{
    bool IsTransactionBoundary { get; set; }
    Task<object> ExecuteAsync(IPipelineServiceProvider provider, object previousResult, CancellationToken ct);
}
