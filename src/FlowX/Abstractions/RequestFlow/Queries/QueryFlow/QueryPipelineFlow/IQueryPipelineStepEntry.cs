namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineStepEntry
{
    Task<object> ExecuteAsync(
        IQueryPipelineServiceProvider provider, object previousResult, CancellationToken ct);
}
