using System.Linq.Expressions;
using FlowX.ApplicationModels;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Delegates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.EntityFrameworkCore.Repositories;

internal class EfRepository<TModel>(IServiceProvider serviceProvider) : IRepository<TModel> where TModel : class
{
    private readonly DbSet<TModel> _collection =
        serviceProvider.GetRequiredService<GetDbContext>().Invoke(typeof(TModel)).Set<TModel>();

    public IQueryable<TModel> GetQueryable(Expression<Func<TModel, bool>> conditionExpression = null) =>
        conditionExpression is null ? _collection : _collection.Where(conditionExpression);

    public Task<TModel> GetFirstByConditionAsync(Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null,
        CancellationToken token = default)
    {
        var dataWithSpecialAction = specialAction?.Invoke(_collection) ?? _collection;
        return conditionExpression is null
            ? dataWithSpecialAction.FirstOrDefaultAsync(token)
            : dataWithSpecialAction.FirstOrDefaultAsync(conditionExpression, token);
    }

    public Task<bool> ExistByConditionAsync(Expression<Func<TModel, bool>> conditionExpression = null,
        CancellationToken token = default)
    {
        return conditionExpression is null
            ? _collection.AsNoTracking().AnyAsync(token)
            : _collection.AsNoTracking().AnyAsync(conditionExpression, token);
    }

    public async Task<IEnumerable<TModel>> GetManyByConditionAsync(
        Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null, CancellationToken token = default)
    {
        var preFilter = _collection.Where(conditionExpression ?? (_ => true));
        var dataWithSpecialAction = specialAction?.Invoke(preFilter) ?? preFilter;
        return await dataWithSpecialAction.ToArrayAsync(token);
    }

    public async Task<Pagination<TModel>> GetManyByConditionWithPaginationAsync(
        Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null,
        CancellationToken token = default)
    {
        var items = await GetManyByConditionAsync(conditionExpression, specialAction, token);
        var totalRecord = await CountByConditionAsync(conditionExpression, specialAction, token);
        return new Pagination<TModel> { Items = items, TotalRecord = totalRecord };
    }

    public Task<long> CountByConditionAsync(Expression<Func<TModel, bool>> conditionExpression = null,
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction = null, CancellationToken token = default)
    {
        var preFilter = _collection.Where(conditionExpression ?? (_ => true));
        var dataWithSpecialAction = specialAction?.Invoke(preFilter) ?? preFilter;
        return dataWithSpecialAction.LongCountAsync(token);
    }

    async Task<TModel> IRepository<TModel>.CreateOneAsync(TModel item, CancellationToken token)
    {
        var result = await _collection.AddAsync(item, token);
        return result.Entity;
    }

    async Task<IEnumerable<TModel>> IRepository<TModel>.CreateManyAsync(IEnumerable<TModel> items,
        CancellationToken token)
    {
        var itemsCreating = items.ToArray();
        await _collection.AddRangeAsync(itemsCreating, token);
        return itemsCreating;
    }

    public async Task<TModel> RemoveOneAsync(Expression<Func<TModel, bool>> filter, CancellationToken token = default)
    {
        var item = await _collection.FirstOrDefaultAsync(filter, token);
        if (item is null) throw new NullReferenceException();
        var result = _collection.Remove(item);
        return result.Entity;
    }

    public async Task<TModel> RemoveOneAsync(TModel item, CancellationToken token = default)
    {
        await Task.Yield();
        var result = _collection.Remove(item);
        return result.Entity;
    }

    public async Task<IEnumerable<TModel>> RemoveManyAsync(Expression<Func<TModel, bool>> filter,
        CancellationToken token = default)
    {
        var items = await _collection.Where(filter).ToArrayAsync(cancellationToken: token);
        _collection.RemoveRange(items);
        return items;
    }


    public async Task<IEnumerable<TModel>> RemoveManyAsync(IEnumerable<TModel> items, CancellationToken token = default)
    {
        await Task.Yield();
        var itemsRemoving = items.ToArray();
        _collection.RemoveRange(itemsRemoving);
        return itemsRemoving;
    }
}