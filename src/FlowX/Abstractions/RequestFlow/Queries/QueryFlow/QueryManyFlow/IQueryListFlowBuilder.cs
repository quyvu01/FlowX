namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListFlowBuilder<TResponse>
{
    Func<Task> BeforeExecutionFunc { get; }
    Func<Task> AfterExecutionFunc { get; }

    Task<(List<TResponse> Items, long TotalCount)> ExecutePaginationAsync(
        IQueryFlowServiceProvider provider,
        string sortedFields, int? skip, int? take,
        CancellationToken ct);

    Task<List<TResponse>> ExecuteCollectionAsync(
        IQueryFlowServiceProvider provider,
        CancellationToken ct);
}
