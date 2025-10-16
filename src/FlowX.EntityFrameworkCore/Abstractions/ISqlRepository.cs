using System.Linq.Expressions;
using FlowX.ApplicationModels;

namespace FlowX.EntityFrameworkCore.Abstractions;

public interface ISqlRepository<T> where T : class
{
    IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null);

    Task<T> GetFirstByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<bool> ExistByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        CancellationToken token = default);

    Task<List<T>> GetManyByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<Pagination<T>> GetManyByConditionWithPaginationAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<long> CountByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<T> CreateOneAsync(T item, CancellationToken token = default);

    Task<List<T>> CreateManyAsync(List<T> items, CancellationToken token = default);

    Task<T> RemoveOneAsync(Expression<Func<T, bool>> filter, CancellationToken token = default);

    Task<T> RemoveOneAsync(T item, CancellationToken token = default);
    Task<List<T>> RemoveManyAsync(Expression<Func<T, bool>> filter, CancellationToken token = default);
    Task<List<T>> RemoveManyAsync(List<T> items, CancellationToken token = default);
}