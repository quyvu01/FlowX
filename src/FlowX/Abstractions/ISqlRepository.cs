using System.Linq.Expressions;
using FlowX.ApplicationModels;
using FlowX.Structs;

namespace FlowX.Abstractions;

public interface ISqlRepository<T> where T : class
{
    IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null);
    IQueryable<T> GetQueryableFromRawQuery(string sql, params object[] parameters);

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

    Task<OneOf<T, Exception>> CreateOneAsync(T item, CancellationToken token = default);

    Task<OneOf<None, Exception>> CreateManyAsync(List<T> items, CancellationToken token = default);

    Task<OneOf<None, Exception>> RemoveOneAsync(OneOf<T, Expression<Func<T, bool>>> itemOrFilter,
        CancellationToken token = default);

    Task<OneOf<None, Exception>> RemoveManyAsync(OneOf<List<T>, Expression<Func<T, bool>>> itemsOrFilter,
        CancellationToken token = default);
}