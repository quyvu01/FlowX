using System.Linq.Expressions;
using FlowX.Extensions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public class QueryManyFlow<TModel, TResponse> :
    IQueryListFilter<TModel, TResponse>,
    IQueryListAfterFilter<TModel, TResponse>,
    IQueryListSpecialAction<TModel, TResponse>,
    IQueryListMapResponse<TModel, TResponse>,
    IQueryListSortedField<TModel, TResponse>,
    IQueryListAfterBuild<TModel, TResponse>,
    IQueryListFlowBuilder<TModel, TResponse> where TModel : class
{
    public QuerySpecialActionType QuerySpecialActionType { get; private set; }
    public Expression<Func<TModel, bool>> Filter { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialActionToModel { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TResponse>> SpecialActionToResponse { get; private set; }
    public Func<TModel, TResponse> MapFunc { get; private set; }
    public ExpressionOrder<TModel> ExpressionOrder { get; private set; }
    public Func<Task> BeforeExecutionFunc { get; private set; }
    public Func<Task> AfterExecutionFunc { get; private set; }

    // === IQueryListFilter (first filter) ===

    IQueryListAfterFilter<TModel, TResponse> IQueryListFilter<TModel, TResponse>.WithFilter(
        Expression<Func<TModel, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    // === IQueryListAfterFilter (chain filters) ===

    IQueryListAfterFilter<TModel, TResponse> IQueryListAfterFilter<TModel, TResponse>.WithFilter(
        Expression<Func<TModel, bool>> filter)
    {
        Filter = Filter.AndAlso(filter);
        return this;
    }

    // === IQueryListSpecialAction ===

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

    // === IQueryListMapResponse ===

    public IQueryListSortedField<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc)
    {
        MapFunc = mapFunc;
        return this;
    }

    // === IQueryListSortedField ===

    IQueryListAfterBuild<TModel, TResponse> IQueryListSortedField<TModel, TResponse>.WithDefaultSortFields(
        ExpressionOrder<TModel> expressionOrder)
    {
        ExpressionOrder = expressionOrder;
        return this;
    }

    IQueryListSortedField<TModel, TResponse> IQueryListSortedField<TModel, TResponse>.WithBeforeExecution(
        Action action)
    {
        BeforeExecutionFunc = () =>
        {
            action.Invoke();
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryListSortedField<TModel, TResponse> IQueryListSortedField<TModel, TResponse>.WithBeforeExecution(
        Func<Task> actionAsync)
    {
        BeforeExecutionFunc = actionAsync;
        return this;
    }

    // === IQueryListAfterBuild ===

    IQueryListFlowBuilder<TModel, TResponse> IQueryListAfterBuild<TModel, TResponse>.WithAfterExecution(
        Action action)
    {
        AfterExecutionFunc = () =>
        {
            action.Invoke();
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryListFlowBuilder<TModel, TResponse> IQueryListAfterBuild<TModel, TResponse>.WithAfterExecution(
        Func<Task> actionAsync)
    {
        AfterExecutionFunc = actionAsync;
        return this;
    }
}
