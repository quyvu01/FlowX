namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public interface ICountingFlowBuilder
{
    Task<long> ExecuteCountAsync(IQueryFlowServiceProvider provider, CancellationToken ct);
}
