using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineServiceProvider
{
    Task<TModel> GetFirstByConditionAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
        CancellationToken ct) where TModel : class;

    Task<List<TModel>> GetManyByConditionAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
        CancellationToken ct) where TModel : class;

    Task<QueryPipelinePage<TModel>> GetManyWithPaginationAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
        ExpressionOrder<TModel> defaultSort,
        string sortedFields,
        int? skip, int? take,
        CancellationToken ct) where TModel : class;
}
