using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryPipelineCountingConfigurator<TModel, TPrev> :
    IQueryPipelineCountingStep<TModel>
    where TModel : class
{
    private readonly QueryPipelineFlow _pipeline;
    private readonly QueryCountingPipelineStep<TModel, TPrev> _currentStep;

    internal QueryPipelineCountingConfigurator(
        QueryPipelineFlow pipeline, QueryCountingPipelineStep<TModel, TPrev> currentStep)
    {
        _pipeline = pipeline;
        _currentStep = currentStep;
    }

    // ===== IQueryPipelineCountingStep<TModel> =====

    IQueryPipelineCountingStep<TModel> IQueryPipelineCountingStep<TModel>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        _currentStep.SpecialAction = specialAction;
        return this;
    }

    // ===== IQueryPipelineNextable<long> — Filter-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<long>.ThenQueryOne<TNext>(
        Func<long, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryOnePipelineStep<TNext, long> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, long>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<long>.ThenQueryMany<TNext>(
        Func<long, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryManyPipelineStep<TNext, long> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, long>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<long> — Queryable-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<long>.ThenQueryOneFromQueryable<TNext>(
        Func<long, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryOnePipelineStep<TNext, long>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, long>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<long>.ThenQueryManyFromQueryable<TNext>(
        Func<long, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryManyPipelineStep<TNext, long>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, long>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<long> — Paginated =====

    IQueryPipelinePaginatedStep<TNext> IQueryPipelineNextable<long>.ThenQueryPaginated<TNext>(
        Func<long, Expression<Func<TNext, bool>>> filterFactory,
        int? skip, int? take, string sortedFields)
    {
        var step = new QueryPaginatedPipelineStep<TNext, long>
        {
            FilterFactory = filterFactory, Skip = skip, Take = take, SortedFields = sortedFields
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelinePaginatedConfigurator<TNext, long>(_pipeline, step);
    }

    IQueryPipelinePaginatedStep<TNext> IQueryPipelineNextable<long>.ThenQueryPaginatedFromQueryable<TNext>(
        Func<long, IQueryable<TNext>, IQueryable<TNext>> queryableFactory,
        int? skip, int? take, string sortedFields)
    {
        var step = new QueryPaginatedPipelineStep<TNext, long>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q),
            Skip = skip, Take = take, SortedFields = sortedFields
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelinePaginatedConfigurator<TNext, long>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<long> — Counting =====

    IQueryPipelineCountingStep<TNext> IQueryPipelineNextable<long>.ThenQueryCounting<TNext>(
        Func<long, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryCountingPipelineStep<TNext, long> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineCountingConfigurator<TNext, long>(_pipeline, step);
    }

    IQueryPipelineCountingStep<TNext> IQueryPipelineNextable<long>.ThenQueryCountingFromQueryable<TNext>(
        Func<long, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryCountingPipelineStep<TNext, long>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineCountingConfigurator<TNext, long>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<long> — Terminal =====

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<long>.WithResult<TResult>(
        Func<long, TResult> resultFunc)
        => ((IQueryPipelineNextable<long>)this).WithResult<TResult>(
            count => Task.FromResult(resultFunc(count)));

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<long>.WithResult<TResult>(
        Func<long, Task<TResult>> resultFuncAsync)
    {
        _pipeline.ResultFuncAsyncValue = new Func<object, Task<TResult>>(
            obj => resultFuncAsync((long)obj));
        return new QueryPipelineResultConfigurator<TResult>(_pipeline);
    }
}
