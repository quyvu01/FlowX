using System.Linq.Expressions;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;

public abstract class EfQueryManyBased<TQuery, TResponse>
    where TResponse : class
{
    protected abstract IQueryListFlowBuilder<TResponse> BuildQueryFlow(
        IQueryListFilter<TResponse> fromFlow, IRequestContext<TQuery> queryContext);

    protected static ExpressionOrder<TModel> Asc<TModel>(Expression<Func<TModel, object>> orderExpression) =>
        ExpressionOrder<TModel>.Of(orderExpression);

    protected static ExpressionOrder<TModel> Desc<TModel>(Expression<Func<TModel, object>> orderExpression) =>
        ExpressionOrder<TModel>.Of(orderExpression, false);
}
