using System.Linq.Expressions;

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
}
