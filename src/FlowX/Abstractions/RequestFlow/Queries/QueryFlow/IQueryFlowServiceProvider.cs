using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow;

public interface IQueryFlowServiceProvider
{
    Task<List<TModel>> ToListAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> transform,
        CancellationToken ct) where TModel : class;

    Task<List<TResult>> ToListAsync<TModel, TResult>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TResult>> transform,
        CancellationToken ct) where TModel : class;

    Task<TModel> FirstOrDefaultAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> transform,
        CancellationToken ct) where TModel : class;

    Task<TResult> FirstOrDefaultAsync<TModel, TResult>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TResult>> transform,
        CancellationToken ct) where TModel : class;

    Task<long> LongCountAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> transform,
        CancellationToken ct) where TModel : class;
}
