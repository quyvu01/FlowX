using System.Linq.Expressions;
using System.Reflection;
using FlowX.Structs;

namespace FlowX.Extensions;

public static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> src, Action<T> action)
    {
        foreach (var item in src ?? []) action?.Invoke(item);
    }

    public static void IteratorVoid<T>(this IEnumerable<T> src) => src.ForEach(_ => { });

    public static IOrderedQueryable<T> OrderByWithDynamic<T>(this IQueryable<T> src, string fieldName,
        Expression<Func<T, object>> defaultOrError, SortedDirection sortedDirection)
    {
        Func<Expression<Func<T, object>>, SortedDirection?, IOrderedQueryable<T>> getOrdered =
            (expression, direction) => direction is SortedDirection.Ascending
                ? src.OrderBy(expression)
                : src.OrderByDescending(expression);
        if (string.IsNullOrEmpty(fieldName)) return getOrdered.Invoke(defaultOrError, sortedDirection);
        try
        {
            return getOrdered.Invoke(BuildPropertyExpression<T>(fieldName), sortedDirection);
        }
        catch (Exception)
        {
            return getOrdered.Invoke(defaultOrError, sortedDirection);
        }
    }

    private static Expression<Func<T, object>> BuildPropertyExpression<T>(string propertyName)
    {
        //Todo: update this one later to improve the performance, consider update or not!
        var type = typeof(T);
        var property = type.GetProperty(propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
            throw new ArgumentException($"Property '{propertyName}' not found on type '{type.Name}'.");
        var parameter = Expression.Parameter(type, "x");
        var propertyAccess = Expression.Property(parameter, property);
        var convertedProperty = Expression.Convert(propertyAccess, typeof(object));
        return Expression.Lambda<Func<T, object>>(convertedProperty, parameter);
    }

    public static IQueryable<T> Offset<T>(this IQueryable<T> src, int? skip) =>
        skip is null ? src : src.Skip(skip.Value);

    public static IQueryable<T> Limit<T>(this IQueryable<T> src, int? take) =>
        take is null ? src : src.Take(take.Value);
}