namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public sealed class QueryPipelinePage<TModel> where TModel : class
{
    public List<TModel> Items { get; init; }
    public long TotalRecord { get; init; }
}
