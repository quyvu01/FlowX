using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public sealed class ExpressionOrder<TModel>
{
    private readonly List<ExpressionOrderDetail<TModel>> _expressionDetails = [];

    private ExpressionOrder(Expression<Func<TModel, object>> orderExpression, bool isAsc = true)
    {
        _expressionDetails.Add(new ExpressionOrderDetail<TModel>(orderExpression, isAsc));
    }

    public ExpressionOrder<TModel> ThenBy(Expression<Func<TModel, object>> orderExpression)
    {
        _expressionDetails.Add(new ExpressionOrderDetail<TModel>(orderExpression, true));
        return this;
    }

    public ExpressionOrder<TModel> ThenDescBy(Expression<Func<TModel, object>> orderExpression)
    {
        _expressionDetails.Add(new ExpressionOrderDetail<TModel>(orderExpression, false));
        return this;
    }

    public static ExpressionOrder<TModel> Of(Expression<Func<TModel, object>> orderExpression, bool isAsc = true) =>
        new(orderExpression, isAsc);

    public IReadOnlyList<ExpressionOrderDetail<TModel>> ExpressionDetails => _expressionDetails;

    public static implicit operator ExpressionOrder<TModel>(Expression<Func<TModel, object>> expression) =>
        new(expression);
}

public sealed record ExpressionOrderDetail<TModel>(Expression<Func<TModel, object>> Expression, bool IsAsc);