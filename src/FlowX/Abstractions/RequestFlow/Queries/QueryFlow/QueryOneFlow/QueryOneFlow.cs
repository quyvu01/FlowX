using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Extensions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public class QueryOneFlow<TModel, TResponse> :
    IQueryOneFilter<TModel, TResponse>,
    IQueryOneAfterFilter<TModel, TResponse>,
    IQueryOneSpecialAction<TModel, TResponse>,
    IQueryOneMapResponse<TModel, TResponse>,
    IQueryOneErrorDetail<TModel, TResponse>,
    IQueryOneAfterBuild<TModel, TResponse>,
    IQueryOneFlowBuilder<TModel, TResponse> where TModel : class where TResponse : class
{
    public QuerySpecialActionType QuerySpecialActionType { get; private set; }
    public Expression<Func<TModel, bool>> Filter { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialAction { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TResponse>> SpecialActionToResponse { get; private set; }
    public Func<TModel, TResponse> MapFunc { get; private set; }
    public Error Error { get; private set; }
    public Func<Task> BeforeExecutionFunc { get; private set; }
    public Func<TResponse, Task> AfterExecutionFunc { get; private set; }

    // === IQueryOneFilter (first filter) ===

    IQueryOneAfterFilter<TModel, TResponse> IQueryOneFilter<TModel, TResponse>.WithFilter(
        Expression<Func<TModel, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    // === IQueryOneAfterFilter (chain filters) ===

    IQueryOneAfterFilter<TModel, TResponse> IQueryOneAfterFilter<TModel, TResponse>.WithFilter(
        Expression<Func<TModel, bool>> filter)
    {
        Filter = Filter.AndAlso(filter);
        return this;
    }

    // === IQueryOneSpecialAction ===

    IQueryOneMapResponse<TModel, TResponse> IQueryOneSpecialAction<TModel, TResponse>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToModel;
        SpecialAction = specialAction;
        return this;
    }

    public IQueryOneErrorDetail<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToTarget;
        SpecialActionToResponse = specialAction;
        return this;
    }

    // === IQueryOneMapResponse ===

    public IQueryOneErrorDetail<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc)
    {
        MapFunc = mapFunc;
        return this;
    }

    // === IQueryOneErrorDetail ===

    IQueryOneAfterBuild<TModel, TResponse> IQueryOneErrorDetail<TModel, TResponse>.WithErrorIfNull(
        [NotNull] Error error)
    {
        Error = error;
        return this;
    }

    IQueryOneErrorDetail<TModel, TResponse> IQueryOneErrorDetail<TModel, TResponse>.WithBeforeExecution(
        Action action)
    {
        BeforeExecutionFunc = () =>
        {
            action.Invoke();
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryOneErrorDetail<TModel, TResponse> IQueryOneErrorDetail<TModel, TResponse>.WithBeforeExecution(
        Func<Task> actionAsync)
    {
        BeforeExecutionFunc = actionAsync;
        return this;
    }

    // === IQueryOneAfterBuild ===

    IQueryOneFlowBuilder<TModel, TResponse> IQueryOneAfterBuild<TModel, TResponse>.WithAfterExecution(
        Action<TResponse> action)
    {
        AfterExecutionFunc = response =>
        {
            action.Invoke(response);
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryOneFlowBuilder<TModel, TResponse> IQueryOneAfterBuild<TModel, TResponse>.WithAfterExecution(
        Func<TResponse, Task> actionAsync)
    {
        AfterExecutionFunc = actionAsync;
        return this;
    }
}
