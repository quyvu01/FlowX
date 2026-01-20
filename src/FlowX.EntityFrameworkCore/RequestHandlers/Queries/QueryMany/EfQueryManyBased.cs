using System.Linq.Expressions;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;

public abstract class EfQueryManyBased<TModel, TQuery, TResponse>
    where TModel : class
    where TResponse : class
{
    protected abstract IQueryListFlowBuilder<TModel, TResponse> BuildQueryFlow(
        IQueryListFilter<TModel, TResponse> fromFlow, IRequestContext<TQuery> queryContext);

    protected ExpressionOrder<TModel> Asc(Expression<Func<TModel, object>> orderExpression) =>
        ExpressionOrder<TModel>.Of(orderExpression);

    protected ExpressionOrder<TModel> Desc(Expression<Func<TModel, object>> orderExpression) =>
        ExpressionOrder<TModel>.Of(orderExpression, false);
}