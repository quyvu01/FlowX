using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryPipelineOneConfigurator<TModel, TPrev> :
    IQueryPipelineOneStep<TModel>
    where TModel : class
{
    private readonly QueryPipelineFlow _pipeline;
    private readonly QueryOnePipelineStep<TModel, TPrev> _currentStep;

    internal QueryPipelineOneConfigurator(QueryPipelineFlow pipeline, QueryOnePipelineStep<TModel, TPrev> currentStep)
    {
        _pipeline = pipeline;
        _currentStep = currentStep;
    }

    // ===== IQueryPipelineOneStep<TModel> =====

    IQueryPipelineOneStep<TModel> IQueryPipelineOneStep<TModel>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        _currentStep.SpecialAction = specialAction;
        return this;
    }

    IQueryPipelineOneStep<TModel> IQueryPipelineOneStep<TModel>.WithErrorIfNull(Error error)
    {
        _currentStep.NullError = error;
        return this;
    }

    // ===== IQueryPipelineNextable<TModel> — Filter-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<TModel>.ThenQueryOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryOnePipelineStep<TNext, TModel> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, TModel>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<TModel>.ThenQueryMany<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new QueryManyPipelineStep<TNext, TModel> { FilterFactory = filterFactory };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, TModel>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<TModel> — Queryable-based =====

    IQueryPipelineOneStep<TNext> IQueryPipelineNextable<TModel>.ThenQueryOneFromQueryable<TNext>(
        Func<TModel, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryOnePipelineStep<TNext, TModel>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TNext, TModel>(_pipeline, step);
    }

    IQueryPipelineManyStep<TNext> IQueryPipelineNextable<TModel>.ThenQueryManyFromQueryable<TNext>(
        Func<TModel, IQueryable<TNext>, IQueryable<TNext>> queryableFactory)
    {
        var step = new QueryManyPipelineStep<TNext, TModel>
        {
            SpecialActionFactory = prev => q => queryableFactory(prev, q)
        };
        _pipeline.StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TNext, TModel>(_pipeline, step);
    }

    // ===== IQueryPipelineNextable<TModel> — Terminal =====

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<TModel>.WithResult<TResult>(
        Func<TModel, TResult> resultFunc)
        => ((IQueryPipelineNextable<TModel>)this).WithResult<TResult>(
            model => Task.FromResult(resultFunc(model)));

    IQueryPipelineResultTerminal<TResult> IQueryPipelineNextable<TModel>.WithResult<TResult>(
        Func<TModel, Task<TResult>> resultFuncAsync)
    {
        _pipeline.ResultFuncAsyncValue = new Func<object, Task<TResult>>(
            obj => resultFuncAsync((TModel)obj));
        return new QueryPipelineResultConfigurator<TResult>(_pipeline);
    }
}
