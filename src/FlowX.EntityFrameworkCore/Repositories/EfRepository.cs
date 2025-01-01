using System.Linq.Expressions;
using FlowX.Abstractions;
using FlowX.ApplicationModels;
using FlowX.Structs;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Repositories;

public class EfRepository<T> : ISqlRepository<T> where T : class
{
    private readonly DbSet<T> _collection;

    protected EfRepository(DbContext dbContext) => _collection = dbContext.Set<T>();

    public virtual IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null) =>
        conditionExpression is null ? _collection : _collection.Where(conditionExpression);

    public virtual Task<T> GetFirstByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null,
        CancellationToken token = default)
    {
        var dataWithSpecialAction = specialAction?.Invoke(_collection) ?? _collection;
        return conditionExpression is null
            ? dataWithSpecialAction.FirstOrDefaultAsync(token)
            : dataWithSpecialAction.FirstOrDefaultAsync(conditionExpression, token);
    }

    public virtual Task<bool> ExistByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        CancellationToken token = default)
    {
        return conditionExpression is null
            ? _collection.AsNoTracking().AnyAsync(token)
            : _collection.AsNoTracking().AnyAsync(conditionExpression, token);
    }

    public virtual Task<List<T>> GetManyByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default)
    {
        var preFilter = _collection.Where(conditionExpression ?? (_ => true));
        var dataWithSpecialAction = specialAction?.Invoke(preFilter) ?? preFilter;
        return dataWithSpecialAction.ToListAsync(token);
    }

    public virtual async Task<Pagination<T>> GetManyByConditionWithPaginationAsync(
        Expression<Func<T, bool>> conditionExpression = null, Func<IQueryable<T>, IQueryable<T>> specialAction = null,
        CancellationToken token = default)
    {
        var items = await GetManyByConditionAsync(conditionExpression, specialAction, token);
        var totalRecord = await CountByConditionAsync(conditionExpression, specialAction, token);
        return new Pagination<T> { Items = items, TotalRecord = totalRecord };
    }

    public virtual Task<long> CountByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default)
    {
        var preFilter = _collection.Where(conditionExpression ?? (_ => true));
        var dataWithSpecialAction = specialAction?.Invoke(preFilter) ?? preFilter;
        return dataWithSpecialAction.LongCountAsync(token);
    }


    public virtual Task<OneOf<T, Exception>> CreateOneAsync(T item, CancellationToken token = default)
    {
        if (item is null) return Task.FromResult(OneOf<T, Exception>.FromT0(null));
        try
        {
            var result = _collection.Add(item);
            return Task.FromResult(OneOf<T, Exception>.FromT0(result.Entity));
        }
        catch (Exception e)
        {
            return Task.FromResult(OneOf<T, Exception>.FromT1(e));
        }
    }

    public virtual Task<OneOf<None, Exception>> CreateManyAsync(List<T> items, CancellationToken token = default)
    {
        if (items is not { Count: > 0 }) return Task.FromResult(OneOf<None, Exception>.FromT0(None.Value));
        try
        {
            _collection.AddRange(items);
            return Task.FromResult(OneOf<None, Exception>.FromT0(None.Value));
        }
        catch (Exception e)
        {
            return Task.FromResult(OneOf<None, Exception>.FromT1(e));
        }
    }

    public virtual async Task<OneOf<None, Exception>> RemoveOneAsync(OneOf<T, Expression<Func<T, bool>>> itemOrFilter,
        CancellationToken token = default)
    {
        var item = await itemOrFilter.Match(Task.FromResult,
            filter => GetFirstByConditionAsync(filter, null, token));
        if (item is null) return None.Value;
        try
        {
            _collection.Remove(item);
            return None.Value;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public virtual async Task<OneOf<None, Exception>> RemoveManyAsync(
        OneOf<List<T>, Expression<Func<T, bool>>> itemsOrFilter, CancellationToken token = default)
    {
        var items = await itemsOrFilter.Match(Task.FromResult,
            filter => GetManyByConditionAsync(filter, null, token));
        if (items is not { Count: > 0 }) return None.Value;
        try
        {
            _collection.RemoveRange(items);
            return None.Value;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}