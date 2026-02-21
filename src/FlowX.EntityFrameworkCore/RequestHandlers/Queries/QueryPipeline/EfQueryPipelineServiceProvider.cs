using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryPipeline;

internal sealed class EfQueryPipelineServiceProvider(IUnitOfWork unitOfWork) : IQueryPipelineServiceProvider
{
    public async Task<TModel> GetFirstByConditionAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
        CancellationToken ct) where TModel : class
    {
        var repository = unitOfWork.RepositoryOf<TModel>();
        return await repository.GetFirstByConditionAsync(filter,
            q => (specialAction?.Invoke(q) ?? q).AsNoTracking(), ct);
    }

    public async Task<List<TModel>> GetManyByConditionAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
        CancellationToken ct) where TModel : class
    {
        var repository = unitOfWork.RepositoryOf<TModel>();
        var items = await repository.GetManyByConditionAsync(filter,
            q => (specialAction?.Invoke(q) ?? q).AsNoTracking(), ct);
        return items.ToList();
    }

    public async Task<QueryPipelinePage<TModel>> GetManyWithPaginationAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
        ExpressionOrder<TModel> defaultSort,
        string sortedFields,
        int? skip, int? take,
        CancellationToken ct) where TModel : class
    {
        var repository = unitOfWork.RepositoryOf<TModel>();
        var queryable = repository.GetQueryable(filter).AsNoTracking();

        var sortExpressions = defaultSort?.ExpressionDetails;
        var ordered = queryable.OrderDynamicOrDefault(sortedFields, sortExpressions);

        var finalQueryable = specialAction is not null ? specialAction(ordered) : ordered;

        var totalRecord = await finalQueryable.LongCountAsync(ct);
        var items = await finalQueryable.Offset(skip).Limit(take).ToListAsync(ct);

        return new QueryPipelinePage<TModel> { Items = items, TotalRecord = totalRecord };
    }

    public async Task<long> GetCountAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
        CancellationToken ct) where TModel : class
    {
        var repository = unitOfWork.RepositoryOf<TModel>();
        return await repository.CountByConditionAsync(filter,
            q => (specialAction?.Invoke(q) ?? q).AsNoTracking(), ct);
    }
}
