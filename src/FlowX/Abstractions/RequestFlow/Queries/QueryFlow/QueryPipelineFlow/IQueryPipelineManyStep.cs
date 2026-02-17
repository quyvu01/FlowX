using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineManyStep<TModel> : IQueryPipelineNextable<List<TModel>> where TModel : class
{
    IQueryPipelineManyStep<TModel> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);

    IQueryPipelineManyStep<TModel> WithDefaultSortFields(ExpressionOrder<TModel> expressionOrder);
}
