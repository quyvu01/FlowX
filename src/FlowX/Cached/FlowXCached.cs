using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FlowX.Cached;

public static class FlowXCached
{
    private static readonly Lazy<ConcurrentDictionary<Type, Func<object[], object>>> ConstructorCache = new(() => []);
    public static IReadOnlyDictionary<Type, Type> RequestMapResponse => InternalRequestMapResponse.Value;
    internal static readonly Lazy<Dictionary<Type, Type>> InternalRequestMapResponse = new(() => []);

    public static object CreateInstanceWithCache(Type type, params object[] args) =>
        ConstructorCache.Value.GetOrAdd(type, t =>
        {
            var constructor = t.GetConstructors().FirstOrDefault();
            if (constructor is null) throw new InvalidOperationException("No matching constructor found.");
            var parameters = Expression.Parameter(typeof(object[]), nameof(args));
            var arguments = constructor
                .GetParameters()
                .Select((p, index) => Expression
                    .Convert(Expression.ArrayIndex(parameters, Expression.Constant(index)), p.ParameterType))
                .ToArray<Expression>();
            var newExpression = Expression.New(constructor, arguments);
            var lambda = Expression.Lambda<Func<object[], object>>(newExpression, parameters);
            return lambda.Compile();
        })(args);
}