using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Extensions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public class QueryOneFlowStart<TResponse> : IQueryOneFilter<TResponse> where TResponse : class
{
    public IQueryOneAfterFilter<TModel, TResponse> WithFilter<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
        => new QueryOneFlowConfigurator<TModel, TResponse>(filter);
}

internal sealed class QueryOneFlowConfigurator<TModel, TResponse> :
    IQueryOneAfterFilter<TModel, TResponse>,
    IQueryOneSpecialAction<TModel, TResponse>,
    IQueryOneMapResponse<TModel, TResponse>,
    IQueryOneErrorDetail<TModel, TResponse>,
    IQueryOneAfterBuild<TResponse>,
    IQueryOneFlowBuilder<TResponse>
    where TModel : class
    where TResponse : class
{
    private QuerySpecialActionType _querySpecialActionType;
    private Expression<Func<TModel, bool>> _filter;
    private Func<IQueryable<TModel>, IQueryable<TModel>> _specialAction;
    private Func<IQueryable<TModel>, IQueryable<TResponse>> _specialActionToResponse;
    private Func<TModel, TResponse> _mapFunc;

    public Error NullError { get; private set; }
    public Func<Task> BeforeExecutionFunc { get; private set; }
    public Func<TResponse, Task> AfterExecutionFunc { get; private set; }

    internal QueryOneFlowConfigurator(Expression<Func<TModel, bool>> filter)
    {
        _filter = filter;
    }

    // === IQueryOneAfterFilter (chain filters) ===

    IQueryOneAfterFilter<TModel, TResponse> IQueryOneAfterFilter<TModel, TResponse>.WithFilter(
        Expression<Func<TModel, bool>> filter)
    {
        _filter = _filter.AndAlso(filter);
        return this;
    }

    // === IQueryOneSpecialAction ===

    IQueryOneMapResponse<TModel, TResponse> IQueryOneSpecialAction<TModel, TResponse>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        _querySpecialActionType = QuerySpecialActionType.ToModel;
        _specialAction = specialAction;
        return this;
    }

    public IQueryOneErrorDetail<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction)
    {
        _querySpecialActionType = QuerySpecialActionType.ToTarget;
        _specialActionToResponse = specialAction;
        return this;
    }

    // === IQueryOneMapResponse ===

    public IQueryOneErrorDetail<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc)
    {
        _mapFunc = mapFunc;
        return this;
    }

    // === IQueryOneErrorDetail ===

    IQueryOneAfterBuild<TResponse> IQueryOneErrorDetail<TModel, TResponse>.WithErrorIfNull(
        [NotNull] Error error)
    {
        NullError = error;
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

    IQueryOneFlowBuilder<TResponse> IQueryOneAfterBuild<TResponse>.WithAfterExecution(
        Action<TResponse> action)
    {
        AfterExecutionFunc = response =>
        {
            action.Invoke(response);
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryOneFlowBuilder<TResponse> IQueryOneAfterBuild<TResponse>.WithAfterExecution(
        Func<TResponse, Task> actionAsync)
    {
        AfterExecutionFunc = actionAsync;
        return this;
    }

    // === IQueryOneFlowBuilder — Execution ===

    public async Task<TResponse> ExecuteOneAsync(
        IQueryFlowServiceProvider provider,
        CancellationToken ct)
    {
        switch (_querySpecialActionType)
        {
            case QuerySpecialActionType.ToModel:
            {
                var item = await provider.FirstOrDefaultAsync<TModel>(
                    _filter,
                    q => _specialAction is not null ? _specialAction(q) : q,
                    ct);

                if (item is null) throw NullError;
                return _mapFunc(item);
            }
            case QuerySpecialActionType.ToTarget:
            default:
            {
                var item = await provider.FirstOrDefaultAsync(
                    _filter,
                    q => _specialActionToResponse(q),
                    ct);

                return item ?? throw NullError;
            }
        }
    }
}
