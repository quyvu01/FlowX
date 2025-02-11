using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryCounting;

public abstract class EfQueryCountingHandler<TModel, TQuery>(ISqlRepository<TModel> sqlRepository) :
    IQueryHandler<TQuery, CountingResponse>
    where TModel : class
    where TQuery : class, IQueryCounting
{
    protected abstract ICountingFlowBuilder<TModel> BuildQueryFlow(
        ICountingFilter<TModel> fromFlow, IRequestXContext<TQuery> queryXContext);

    public virtual async Task<CountingResponse> HandleAsync(IRequestXContext<TQuery> requestXContext)
    {
        var flowBuilder = BuildQueryFlow(new CountingFlow<TModel>(), requestXContext);
        var count = await sqlRepository
            .CountByConditionAsync(flowBuilder.Filter, null, requestXContext.CancellationToken);
        return new CountingResponse { Count = count };
    }
}