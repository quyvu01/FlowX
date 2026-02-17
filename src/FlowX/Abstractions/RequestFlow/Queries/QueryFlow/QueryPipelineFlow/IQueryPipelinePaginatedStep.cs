using System.Diagnostics.CodeAnalysis;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelinePaginatedStep<TModel>
    : IQueryPipelineNextable<QueryPipelinePage<TModel>> where TModel : class
{
    IQueryPipelinePaginatedStep<TModel> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);

    IQueryPipelinePaginatedStep<TModel> WithDefaultSortFields(
        [NotNull] ExpressionOrder<TModel> expressionOrder);
}
