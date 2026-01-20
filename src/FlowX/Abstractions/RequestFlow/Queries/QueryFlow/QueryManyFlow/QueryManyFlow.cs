using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public class QueryManyFlow<TModel, TResponse> : IQueryListFilter<TModel, TResponse>,
    IQueryListSpecialAction<TModel, TResponse>,
    IQueryListMapResponse<TModel, TResponse>,
    IQueryListSortedField<TModel, TResponse>,
    IQueryListFlowBuilder<TModel, TResponse> where TModel : class
{
    public QuerySpecialActionType QuerySpecialActionType { get; private set; }
    public Expression<Func<TModel, bool>> Filter { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialActionToModel { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TResponse>> SpecialActionToResponse { get; private set; }
    public Func<TModel, TResponse> MapFunc { get; private set; }
    public ExpressionOrder<TModel> ExpressionOrder { get; private set; }

    public IQueryListSpecialAction<TModel, TResponse> WithFilter(Expression<Func<TModel, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    IQueryListMapResponse<TModel, TResponse> IQueryListSpecialAction<TModel, TResponse>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToModel;
        SpecialActionToModel = specialAction;
        return this;
    }

    public IQueryListSortedField<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToTarget;
        SpecialActionToResponse = specialAction;
        return this;
    }

    public IQueryListSortedField<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc)
    {
        MapFunc = mapFunc;
        return this;
    }

    public IQueryListFlowBuilder<TModel, TResponse> WithDefaultSortFields(ExpressionOrder<TModel> expressionOrder)
    {
        ExpressionOrder = expressionOrder;
        return this;
    }
}