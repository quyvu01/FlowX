namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineCountingStep<TModel> : IQueryPipelineNextable<long> where TModel : class
{
    IQueryPipelineCountingStep<TModel> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
}
