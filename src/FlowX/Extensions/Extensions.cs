using System.Linq.Expressions;
using System.Reflection;
using FlowX.DynamicExpression;
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
        Expression<Func<T, object>> defaultOrError,
        SortedDirection sortedDirection)
    {
        Func<Expression<Func<T, object>>, SortedDirection?, IOrderedQueryable<T>> getOrdered =
            (expression, direction) => direction is SortedDirection.Ascending
                ? src.OrderBy(expression)
                : src.OrderByDescending(expression);
        if (string.IsNullOrEmpty(fieldName)) return getOrdered.Invoke(defaultOrError, sortedDirection);
        try
        {
            var interpreter = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);
            var expressionStr = $"x.{fieldName}";
            var expressionFilter = interpreter.ParseAsExpression<Func<T, object>>(expressionStr, "x");
            return getOrdered.Invoke(expressionFilter, sortedDirection);
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

    public static bool IsConcrete(this Type type) => !type.IsAbstract && !type.IsInterface;

    public static bool IsOpenGeneric(this Type type) => type.IsGenericTypeDefinition || type.ContainsGenericParameters;

    public static bool CanBeCastTo(this Type pluggedType, Type pluginType)
    {
        if (pluggedType is null) return false;
        return pluggedType == pluginType || pluginType.IsAssignableFrom(pluggedType);
    }

    public static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
    {
        var openInterface = closedInterface.GetGenericTypeDefinition();
        var arguments = closedInterface.GenericTypeArguments;

        var concreteArguments = openConcretion.GenericTypeArguments;
        return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
    }

    public static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
    {
        return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
    }

    public static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
    {
        if (pluggedType == null) yield break;

        if (!pluggedType.IsConcrete()) yield break;

        if (templateType.IsInterface)
        {
            foreach (
                var interfaceType in
                pluggedType.GetInterfaces()
                    .Where(type => type.IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
            {
                yield return interfaceType;
            }
        }
        else if (pluggedType.BaseType!.IsGenericType &&
                 (pluggedType.BaseType!.GetGenericTypeDefinition() == templateType))
        {
            yield return pluggedType.BaseType!;
        }

        if (pluggedType.BaseType == typeof(object)) yield break;

        foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.BaseType!, templateType))
        {
            yield return interfaceType;
        }
    }

    public static void Fill<T>(this IList<T> list, T value)
    {
        if (list.Contains(value)) return;
        list.Add(value);
    }
}