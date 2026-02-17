using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;
using FlowX.EntityFrameworkCore.Abstractions;
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
}
