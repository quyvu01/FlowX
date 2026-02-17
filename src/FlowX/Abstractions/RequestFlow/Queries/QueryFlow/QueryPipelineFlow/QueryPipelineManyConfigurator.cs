using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryPipelineManyConfigurator<TModel, TPrev> :
    IQueryPipelineManyStep<TModel>
    where TModel : class
{
    private readonly QueryPipelineFlow _pipeline;
    private readonly QueryManyPipelineStep<TModel, TPrev> _currentStep;

    internal QueryPipelineManyConfigurator(QueryPipelineFlow pipeline, QueryManyPipelineStep<TModel, TPrev> currentStep)
    {
        _pipeline = pipeline;
        _currentStep = currentStep;
    }

    // ===== IQueryPipelineManyStep<TModel> =====

    IQueryPipelineManyStep<TModel> IQueryPipelineManyStep<TModel>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        _currentStep.SpecialAction = specialAction;
        return this;
    }

    // ===== IQueryPipelineNextable<List<TModel>> — Filter-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<List<TModel>>.ThenQueryOne<TNext>(
        Func<List<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryOnePipelineStep<TNext, List<TModel>> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, List<TModel>>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<List<TModel>>.ThenQueryMany<TNext>(
        Func<List<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryManyPipelineStep<TNext, List<TModel>> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, List<TModel>>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<List<TModel>> — Queryable-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<List<TModel>>.ThenQueryOneFromQueryable<TNext>(
        Func<List<TModel>, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryOnePipelineStep<TNext, List<TModel>>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, List<TModel>>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<List<TModel>>.ThenQueryManyFromQueryable<TNext>(
        Func<List<TModel>, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryManyPipelineStep<TNext, List<TModel>>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, List<TModel>>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<List<TModel>> — Terminal =====

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<List<TModel>>.WithResult<TResult>(
        Func<List<TModel>, TResult> resultFunc)
        => ((IQueryPipelineNextable<List<TModel>>)this).WithResult(
            list => Task.FromResult(resultFunc(list)));

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<List<TModel>>.WithResult<TResult>(
        Func<List<TModel>, Task<TResult>> resultFuncAsync)
    {
        _pipeline.ResultFuncAsyncValue = new Func<object, Task<TResult>>(
            obj => resultFuncAsync((List<TModel>)obj));
        return new QueryPipelineResultConfigurator<TResult>(_pipeline);
    }
}
