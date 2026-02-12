using System.Linq.Expressions;

namespace FlowX.Extensions;

internal static class ExpressionExtensions
{
    internal static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        if (first is null) return second;
        if (second is null) return first;
        var param = first.Parameters[0];
        var body = Expression.AndAlso(
            first.Body,
            new ParameterReplacer(second.Parameters[0], param).Visit(second.Body));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
    : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
        => node == oldParam ? newParam : base.VisitParameter(node);
}
