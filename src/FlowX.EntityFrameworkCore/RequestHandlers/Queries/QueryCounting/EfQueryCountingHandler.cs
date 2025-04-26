using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryCounting;

public abstract class EfQueryCountingHandler<TModel, TQuery>(ISqlRepository<TModel> sqlRepository) :
    IQueryHandler<TQuery, CountingResponse>
    where TModel : class
    where TQuery : class, IQueryCounting
{
    protected abstract ICountingFlowBuilder<TModel> BuildQueryFlow(
        ICountingFilter<TModel> fromFlow, IRequestContext<TQuery> queryContext);

    public virtual async Task<CountingResponse> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var flowBuilder = BuildQueryFlow(new CountingFlow<TModel>(), requestContext);
        var count = await sqlRepository
            .CountByConditionAsync(flowBuilder.Filter, null, requestContext.CancellationToken);
        return new CountingResponse { Count = count };
    }
}