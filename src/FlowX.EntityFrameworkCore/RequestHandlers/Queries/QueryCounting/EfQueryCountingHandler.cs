using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryCounting;

public abstract class EfQueryCountingHandler<TQuery> :
    IQueryHandler<TQuery, CountingResponse>
    where TQuery : class, IQueryCounting
{
    protected abstract ICountingFlowBuilder BuildQueryFlow(
        ICountingFilter fromFlow, IRequestContext<TQuery> queryContext);

    public virtual async Task<CountingResponse> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var provider = new EfQueryFlowServiceProvider(unitOfWork);
        var flowBuilder = BuildQueryFlow(new CountingFlowStart(), requestContext);
        var count = await flowBuilder.ExecuteCountAsync(provider, requestContext.CancellationToken);
        return new CountingResponse { Count = count };
    }
}
