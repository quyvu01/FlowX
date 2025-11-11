using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryCounting;

public abstract class EfQueryCountingHandler<TModel, TQuery> :
    IQueryHandler<TQuery, CountingResponse>
    where TModel : class
    where TQuery : class, IQueryCounting
{
    protected abstract ICountingFlowBuilder<TModel> BuildQueryFlow(
        ICountingFilter<TModel> fromFlow, IRequestContext<TQuery> queryContext);

    public virtual async Task<CountingResponse> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var flowBuilder = BuildQueryFlow(new CountingFlow<TModel>(), requestContext);
        var count = await unitOfWork.RepositoryOf<TModel>()
            .CountByConditionAsync(flowBuilder.Filter, null, requestContext.CancellationToken);
        return new CountingResponse { Count = count };
    }
}