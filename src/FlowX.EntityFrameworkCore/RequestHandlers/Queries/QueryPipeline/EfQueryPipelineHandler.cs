using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;
using FlowX.EntityFrameworkCore.SharedStates;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryPipeline;

public abstract class EfQueryPipelineHandler<TQuery, TResult>
    : IQueryHandler<TQuery, TResult>
    where TQuery : class, IQuery<TResult>
{
    protected abstract IQueryPipelineFlowBuilder<TResult> BuildPipeline(
        IStartQueryPipeline fromFlow, IRequestContext<TQuery> queryContext);

    public virtual async Task<TResult> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var provider = new EfQueryPipelineServiceProvider(unitOfWork);
        var builder = BuildPipeline(new QueryPipelineFlow(), requestContext);

        if (builder.BeforeExecutionFunc is { } beforeFunc)
            await beforeFunc.Invoke();

        object previousResult = null;
        foreach (var step in builder.Steps)
            previousResult = await step.ExecuteAsync(provider, previousResult, requestContext.CancellationToken);

        if (builder.AfterExecutionFunc is { } afterFunc)
            await afterFunc.Invoke();

        return await builder.ResultFuncAsync(previousResult);
    }
}
