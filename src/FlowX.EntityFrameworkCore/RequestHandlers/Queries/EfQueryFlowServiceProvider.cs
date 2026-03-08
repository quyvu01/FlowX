using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries;

internal sealed class EfQueryFlowServiceProvider(IUnitOfWork unitOfWork) : IQueryFlowServiceProvider
{
    public async Task<List<TModel>> ToListAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> transform,
        CancellationToken ct) where TModel : class
    {
        var q = unitOfWork.RepositoryOf<TModel>().GetQueryable(filter).AsNoTracking();
        if (transform is not null) q = transform(q);
        return await q.ToListAsync(ct);
    }

    public async Task<List<TResult>> ToListAsync<TModel, TResult>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TResult>> transform,
        CancellationToken ct) where TModel : class
    {
        var q = unitOfWork.RepositoryOf<TModel>().GetQueryable(filter).AsNoTracking();
        var projected = transform(q);
        return await projected.ToListAsync(ct);
    }

    public async Task<TModel> FirstOrDefaultAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> transform,
        CancellationToken ct) where TModel : class
    {
        var q = unitOfWork.RepositoryOf<TModel>().GetQueryable(filter).AsNoTracking();
        if (transform is not null) q = transform(q);
        return await q.FirstOrDefaultAsync(ct);
    }

    public async Task<TResult> FirstOrDefaultAsync<TModel, TResult>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TResult>> transform,
        CancellationToken ct) where TModel : class
    {
        var q = unitOfWork.RepositoryOf<TModel>().GetQueryable(filter).AsNoTracking();
        var projected = transform(q);
        return await projected.FirstOrDefaultAsync(ct);
    }

    public async Task<long> LongCountAsync<TModel>(
        Expression<Func<TModel, bool>> filter,
        Func<IQueryable<TModel>, IQueryable<TModel>> transform,
        CancellationToken ct) where TModel : class
    {
        var q = unitOfWork.RepositoryOf<TModel>().GetQueryable(filter).AsNoTracking();
        if (transform is not null) q = transform(q);
        return await q.LongCountAsync(ct);
    }
}
