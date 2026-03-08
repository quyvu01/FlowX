using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;

public abstract class EfQueryCollectionHandler<TQuery, TResponse>
    : EfQueryManyBased<TQuery, TResponse>, IQueryHandler<TQuery, CollectionResponse<TResponse>>
    where TQuery : class, IQueryCollection<TResponse>
    where TResponse : class
{
    public virtual async Task<CollectionResponse<TResponse>> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var provider = new EfQueryFlowServiceProvider(unitOfWork);
        var builder = BuildQueryFlow(new QueryListFlowStart<TResponse>(), requestContext);

        if (builder.BeforeExecutionFunc is { } beforeFunc)
            await beforeFunc.Invoke();

        var items = await builder.ExecuteCollectionAsync(provider, requestContext.CancellationToken);

        if (builder.AfterExecutionFunc is { } afterFunc)
            await afterFunc.Invoke();

        return new CollectionResponse<TResponse>(items);
    }
}
