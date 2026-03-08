using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.EntityFrameworkCore.SharedStates;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;

public abstract class EfQueryOneHandler<TQuery, TResponse>
    : IQueryHandler<TQuery, TResponse>
    where TQuery : class, IQueryOne<TResponse>
    where TResponse : class
{
    protected abstract IQueryOneFlowBuilder<TResponse> BuildQueryFlow(
        IQueryOneFilter<TResponse> fromFlow, IRequestContext<TQuery> queryContext);

    public virtual async Task<TResponse> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var provider = new EfQueryFlowServiceProvider(unitOfWork);
        var builder = BuildQueryFlow(new QueryOneFlowStart<TResponse>(), requestContext);

        if (builder.BeforeExecutionFunc is { } beforeFunc)
            await beforeFunc.Invoke();

        var result = await builder.ExecuteOneAsync(provider, requestContext.CancellationToken);

        if (builder.AfterExecutionFunc is { } afterFunc)
            await afterFunc.Invoke(result);

        return result;
    }
}
