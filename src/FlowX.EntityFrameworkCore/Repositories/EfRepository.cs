using System.Linq.Expressions;
using FlowX.ApplicationModels;
using FlowX.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Repositories;

public class EfRepository<TDbContext, TModel>(TDbContext dbContext)
    : ISqlRepository<TModel> where TDbContext : DbContext
    where TModel : class
{
    private readonly DbSet<TModel> _collection = dbContext.Set<TModel>();

    public virtual IQueryable<TModel> GetQueryable(Expression<Func<TModel, bool>> conditionExpression = null) =>
        conditionExpression is null ? _collection : _collection.Where(conditionExpression);

    public virtual Task<TModel> GetFirstByConditionAsync(Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null,
        CancellationToken token = default)
    {
        var dataWithSpecialAction = specialAction?.Invoke(_collection) ?? _collection;
        return conditionExpression is null
            ? dataWithSpecialAction.FirstOrDefaultAsync(token)
            : dataWithSpecialAction.FirstOrDefaultAsync(conditionExpression, token);
    }

    public virtual Task<bool> ExistByConditionAsync(Expression<Func<TModel, bool>> conditionExpression = null,
        CancellationToken token = default)
    {
        return conditionExpression is null
            ? _collection.AsNoTracking().AnyAsync(token)
            : _collection.AsNoTracking().AnyAsync(conditionExpression, token);
    }

    public virtual Task<List<TModel>> GetManyByConditionAsync(Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null, CancellationToken token = default)
    {
        var preFilter = _collection.Where(conditionExpression ?? (_ => true));
        var dataWithSpecialAction = specialAction?.Invoke(preFilter) ?? preFilter;
        return dataWithSpecialAction.ToListAsync(token);
    }

    public virtual async Task<Pagination<TModel>> GetManyByConditionWithPaginationAsync(
        Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null,
        CancellationToken token = default)
    {
        var items = await GetManyByConditionAsync(conditionExpression, specialAction, token);
        var totalRecord = await CountByConditionAsync(conditionExpression, specialAction, token);
        return new Pagination<TModel> { Items = items, TotalRecord = totalRecord };
    }

    public virtual Task<long> CountByConditionAsync(Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null, CancellationToken token = default)
    {
        var preFilter = _collection.Where(conditionExpression ?? (_ => true));
        var dataWithSpecialAction = specialAction?.Invoke(preFilter) ?? preFilter;
        return dataWithSpecialAction.LongCountAsync(token);
    }

    async Task<TModel> ISqlRepository<TModel>.CreateOneAsync(TModel item, CancellationToken token)
    {
        var result = await _collection.AddAsync(item, token);
        return result.Entity;
    }

    async Task<List<TModel>> ISqlRepository<TModel>.CreateManyAsync(List<TModel> items, CancellationToken token)
    {
        await _collection.AddRangeAsync(items, token);
        return items;
    }

    public async Task<TModel> RemoveOneAsync(Expression<Func<TModel, bool>> filter, CancellationToken token = default)
    {
        var item = await _collection.FirstOrDefaultAsync(filter, token);
        var result = await _collection.AddAsync(item, token);
        return result.Entity;
    }

    public async Task<TModel> RemoveOneAsync(TModel item, CancellationToken token = default)
    {
        var result = await _collection.AddAsync(item, token);
        return result.Entity;
    }

    public async Task<List<TModel>> RemoveManyAsync(Expression<Func<TModel, bool>> filter,
        CancellationToken token = default)
    {
        var items = await _collection.Where(filter).ToListAsync(cancellationToken: token);
        _collection.RemoveRange(items);
        return items;
    }

    public Task<List<TModel>> RemoveManyAsync(List<TModel> items, CancellationToken token = default)
    {
        _collection.RemoveRange(items);
        return Task.FromResult(items);
    }
}