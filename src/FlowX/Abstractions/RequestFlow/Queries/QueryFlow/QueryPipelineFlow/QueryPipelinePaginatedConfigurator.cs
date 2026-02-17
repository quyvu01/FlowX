using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryPipelinePaginatedConfigurator<TModel, TPrev> :
    IQueryPipelinePaginatedStep<TModel>
    where TModel : class
{
    private readonly QueryPipelineFlow _pipeline;
    private readonly QueryPaginatedPipelineStep<TModel, TPrev> _currentStep;

    internal QueryPipelinePaginatedConfigurator(
        QueryPipelineFlow pipeline, QueryPaginatedPipelineStep<TModel, TPrev> currentStep)
    {
        _pipeline = pipeline;
        _currentStep = currentStep;
    }

    // ===== IQueryPipelinePaginatedStep<TModel> =====

    IQueryPipelinePaginatedStep<TModel> IQueryPipelinePaginatedStep<TModel>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        _currentStep.SpecialAction = specialAction;
        return this;
    }

    IQueryPipelinePaginatedStep<TModel> IQueryPipelinePaginatedStep<TModel>.WithDefaultSortFields(
        ExpressionOrder<TModel> expressionOrder)
    {
        _currentStep.DefaultSort = expressionOrder;
        return this;
    }

    // ===== IQueryPipelineNextable<QueryPipelinePage<TModel>> — Filter-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<QueryPipelinePage<TModel>>.ThenQueryOne<TNext>(
        Func<QueryPipelinePage<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryOnePipelineStep<TNext, QueryPipelinePage<TModel>> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, QueryPipelinePage<TModel>>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<QueryPipelinePage<TModel>>.ThenQueryMany<TNext>(
        Func<QueryPipelinePage<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryManyPipelineStep<TNext, QueryPipelinePage<TModel>> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, QueryPipelinePage<TModel>>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<QueryPipelinePage<TModel>> — Queryable-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<QueryPipelinePage<TModel>>.ThenQueryOneFromQueryable<TNext>(
        Func<QueryPipelinePage<TModel>, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryOnePipelineStep<TNext, QueryPipelinePage<TModel>>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, QueryPipelinePage<TModel>>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<QueryPipelinePage<TModel>>.ThenQueryManyFromQueryable<TNext>(
        Func<QueryPipelinePage<TModel>, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryManyPipelineStep<TNext, QueryPipelinePage<TModel>>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, QueryPipelinePage<TModel>>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<QueryPipelinePage<TModel>> — Paginated =====

    IQueryPipelinePaginatedStep<TNext> IQueryPipelineNextable<QueryPipelinePage<TModel>>.ThenQueryPaginated<TNext>(
        Func<QueryPipelinePage<TModel>, Expression<Func<TNext, bool>>> filterFactory,
        int? skip, int? take, string sortedFields)
    {
        var step = new QueryPaginatedPipelineStep<TNext, QueryPipelinePage<TModel>>
        {
            FilterFactory = filterFactory, Skip = skip, Take = take, SortedFields = sortedFields
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelinePaginatedConfigurator<TNext, QueryPipelinePage<TModel>>(_pipeline, step);
    }

    IQueryPipelinePaginatedStep<TNext> IQueryPipelineNextable<QueryPipelinePage<TModel>>.ThenQueryPaginatedFromQueryable<TNext>(
        Func<QueryPipelinePage<TModel>, IQueryable<TNext>, IQueryable<TNext>> queryableFactory,
        int? skip, int? take, string sortedFields)
    {
        var step = new QueryPaginatedPipelineStep<TNext, QueryPipelinePage<TModel>>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q),
            Skip = skip, Take = take, SortedFields = sortedFields
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelinePaginatedConfigurator<TNext, QueryPipelinePage<TModel>>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<QueryPipelinePage<TModel>> — Terminal =====

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<QueryPipelinePage<TModel>>.WithResult<TResult>(
        Func<QueryPipelinePage<TModel>, TResult> resultFunc)
        => ((IQueryPipelineNextable<QueryPipelinePage<TModel>>)this).WithResult(
            page => Task.FromResult(resultFunc(page)));

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<QueryPipelinePage<TModel>>.WithResult<TResult>(
        Func<QueryPipelinePage<TModel>, Task<TResult>> resultFuncAsync)
    {
        _pipeline.ResultFuncAsyncValue = new Func<object, Task<TResult>>(
            obj => resultFuncAsync((QueryPipelinePage<TModel>)obj));
        return new QueryPipelineResultConfigurator<TResult>(_pipeline);
    }
}
