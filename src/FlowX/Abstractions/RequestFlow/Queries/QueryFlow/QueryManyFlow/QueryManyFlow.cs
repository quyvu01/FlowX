using System.Linq.Expressions;
using FlowX.Extensions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public class QueryListFlowStart<TResponse> : IQueryListFilter<TResponse>
{
    public IQueryListAfterFilter<TModel, TResponse> WithFilter<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
        => new QueryListFlowConfigurator<TModel, TResponse>(filter);
}

internal sealed class QueryListFlowConfigurator<TModel, TResponse> :
    IQueryListAfterFilter<TModel, TResponse>,
    IQueryListMapResponse<TModel, TResponse>,
    IQueryListSortedField<TModel, TResponse>,
    IQueryListAfterBuild<TResponse>
    where TModel : class
{
    private QuerySpecialActionType _querySpecialActionType;
    private Expression<Func<TModel, bool>> _filter;
    private Func<IQueryable<TModel>, IQueryable<TModel>> _specialActionToModel;
    private Func<IQueryable<TModel>, IQueryable<TResponse>> _specialActionToResponse;
    private Func<TModel, TResponse> _mapFunc;
    private ExpressionOrder<TModel> _expressionOrder;

    public Func<Task> BeforeExecutionFunc { get; private set; }
    public Func<Task> AfterExecutionFunc { get; private set; }

    internal QueryListFlowConfigurator(Expression<Func<TModel, bool>> filter)
    {
        _filter = filter;
    }

    // === IQueryListAfterFilter (chain filters) ===

    IQueryListAfterFilter<TModel, TResponse> IQueryListAfterFilter<TModel, TResponse>.WithFilter(
        Expression<Func<TModel, bool>> filter)
    {
        _filter = _filter.AndAlso(filter);
        return this;
    }

    // === IQueryListSpecialAction ===

    IQueryListMapResponse<TModel, TResponse> IQueryListSpecialAction<TModel, TResponse>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        _querySpecialActionType = QuerySpecialActionType.ToModel;
        _specialActionToModel = specialAction;
        return this;
    }

    public IQueryListSortedField<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction)
    {
        _querySpecialActionType = QuerySpecialActionType.ToTarget;
        _specialActionToResponse = specialAction;
        return this;
    }

    // === IQueryListMapResponse ===

    public IQueryListSortedField<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc)
    {
        _mapFunc = mapFunc;
        return this;
    }

    // === IQueryListSortedField ===

    IQueryListAfterBuild<TResponse> IQueryListSortedField<TModel, TResponse>.WithDefaultSortFields(
        ExpressionOrder<TModel> expressionOrder)
    {
        _expressionOrder = expressionOrder;
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

    IQueryListFlowBuilder<TResponse> IQueryListAfterBuild<TResponse>.WithAfterExecution(Action action)
    {
        AfterExecutionFunc = () =>
        {
            action.Invoke();
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryListFlowBuilder<TResponse> IQueryListAfterBuild<TResponse>.WithAfterExecution(
        Func<Task> actionAsync)
    {
        AfterExecutionFunc = actionAsync;
        return this;
    }

    // === IQueryListFlowBuilder — Execution ===

    public async Task<(List<TResponse> Items, long TotalCount)> ExecutePaginationAsync(
        IQueryFlowServiceProvider provider,
        string sortedFields, int? skip, int? take,
        CancellationToken ct)
    {
        var sortDetails = _expressionOrder?.ExpressionDetails;

        switch (_querySpecialActionType)
        {
            case QuerySpecialActionType.ToModel:
            {
                var items = await provider.ToListAsync<TModel>(
                    _filter,
                    q =>
                    {
                        var ordered = q.OrderDynamicOrDefault(sortedFields, sortDetails);
                        var afterAction = _specialActionToModel is not null
                            ? _specialActionToModel(ordered)
                            : ordered;
                        return afterAction.Offset(skip).Limit(take);
                    },
                    ct);

                var count = await provider.LongCountAsync(
                    _filter,
                    q => _specialActionToModel is not null ? _specialActionToModel(q) : q,
                    ct);

                var responses = items.Select(a => _mapFunc(a)).ToList();
                return (responses, count);
            }
            case QuerySpecialActionType.ToTarget:
            default:
            {
                var items = await provider.ToListAsync(
                    _filter,
                    q =>
                    {
                        var ordered = q.OrderDynamicOrDefault(sortedFields, sortDetails);
                        return _specialActionToResponse(ordered).Offset(skip).Limit(take);
                    },
                    ct);

                var count = await provider.LongCountAsync(
                    _filter,
                    q => q,
                    ct);

                return (items, count);
            }
        }
    }

    public async Task<List<TResponse>> ExecuteCollectionAsync(
        IQueryFlowServiceProvider provider,
        CancellationToken ct)
    {
        var sortDetails = _expressionOrder?.ExpressionDetails;

        switch (_querySpecialActionType)
        {
            case QuerySpecialActionType.ToModel:
            {
                var items = await provider.ToListAsync<TModel>(
                    _filter,
                    q =>
                    {
                        var afterAction = _specialActionToModel is not null
                            ? _specialActionToModel(q)
                            : q;
                        return sortDetails is not null
                            ? afterAction.OrderDynamicOrDefault(null, sortDetails)
                            : afterAction;
                    },
                    ct);

                return items.Select(a => _mapFunc(a)).ToList();
            }
            case QuerySpecialActionType.ToTarget:
            default:
            {
                return await provider.ToListAsync(
                    _filter,
                    q =>
                    {
                        var ordered = sortDetails is not null
                            ? q.OrderDynamicOrDefault(null, sortDetails)
                            : q;
                        return _specialActionToResponse(ordered);
                    },
                    ct);
            }
        }
    }
}
