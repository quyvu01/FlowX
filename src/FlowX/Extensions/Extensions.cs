using System.Text.RegularExpressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.DynamicExpression;

namespace FlowX.Extensions;

public static partial class Extensions
{
    extension<T>(IEnumerable<T> src)
    {
        public void ForEach(Action<T> action)
        {
            foreach (var item in src ?? []) action?.Invoke(item);
        }

        public void IteratorVoid() => src.ForEach(_ => { });
    }

    extension<T>(IQueryable<T> src)
    {
        public IQueryable<T> Offset(int? skip) =>
            skip is null ? src : src.Skip(skip.Value);

        public IQueryable<T> Limit(int? take) =>
            take is null ? src : src.Take(take.Value);

        public IOrderedQueryable<T> OrderDynamicOrDefault(string sortedFields,
            IReadOnlyList<ExpressionOrderDetail<T>> defaultOrders)
        {
            if (string.IsNullOrEmpty(sortedFields))
            {
                switch (defaultOrders.Count)
                {
                    case 0:
                        throw new ArgumentException("Default Order cannot be empty", nameof(defaultOrders));
                    case 1:
                    {
                        var orderDetail = defaultOrders[0];
                        return orderDetail.IsAsc
                            ? src.OrderBy(orderDetail.Expression)
                            : src.OrderByDescending(orderDetail.Expression);
                    }
                }

                var defaultOrder = defaultOrders[0];
                var orderByQueryable = defaultOrder.IsAsc
                    ? src.OrderBy(defaultOrder.Expression)
                    : src.OrderByDescending(defaultOrder.Expression);
                return defaultOrders.Skip(1).Aggregate(orderByQueryable,
                    (acc, next) => next.IsAsc ? acc.ThenBy(next.Expression) : acc.ThenByDescending(next.Expression));
            }

            var interpreter = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);
            var orderDetails = sortedFields
                .Split(',')
                .Select(a => a.Trim())
                .Select(a =>
                {
                    if (!TryParseOrderBy(a, out var property, out var isAsc))
                        throw new ArgumentException(
                            $"'{a}' is not a valid segment. Only $Property | $Property asc|desc is allowed!");
                    var expressionStr = $"x.{property}";
                    var expressionFilter = interpreter.ParseAsExpression<Func<T, object>>(expressionStr, "x");
                    return new ExpressionOrderDetail<T>(expressionFilter, isAsc);
                });
            return src.OrderDynamicOrDefault(null, [..orderDetails]);
        }
    }

    private static readonly Regex OrderByRegex = MyRegex();

    public static bool TryParseOrderBy(string input, out string property, out bool isAsc)
    {
        property = string.Empty;
        isAsc = true;

        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = OrderByRegex.Match(input.Trim());
        if (!match.Success) return false;

        property = match.Groups["prop"].Value;

        var dir = match.Groups["dir"].Value;
        isAsc = !dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return true;
    }


    extension(Type type)
    {
        public bool IsConcrete() => !type.IsAbstract && !type.IsInterface;
        public bool IsOpenGeneric() => type.IsGenericTypeDefinition || type.ContainsGenericParameters;

        public bool CanBeCastTo(Type pluginType)
        {
            if (type is null) return false;
            return type == pluginType || pluginType.IsAssignableFrom(type);
        }

        public bool CouldCloseTo(Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = type.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && type.CanBeCastTo(openInterface);
        }

        public IEnumerable<Type> FindInterfacesThatClose(Type templateType)
        {
            return FindInterfacesThatClosesCore(type, templateType).Distinct();
        }
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

    [GeneratedRegex(@"^(?<prop>[A-Za-z_][A-Za-z0-9_]*)(\s+(?<dir>asc|desc))?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-VN")]
    private static partial Regex MyRegex();
}