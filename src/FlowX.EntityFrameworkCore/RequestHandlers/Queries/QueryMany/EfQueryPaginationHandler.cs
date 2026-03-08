using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;

public abstract class EfQueryPaginationHandler<TQuery, TResponse>
    : EfQueryManyBased<TQuery, TResponse>, IQueryHandler<TQuery, PaginationResponse<TResponse>>
    where TQuery : GetManyQuery, IQueryPaged<TResponse>
    where TResponse : class
{
    public virtual async Task<PaginationResponse<TResponse>> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var provider = new EfQueryFlowServiceProvider(unitOfWork);
        var builder = BuildQueryFlow(new QueryListFlowStart<TResponse>(), requestContext);

        if (builder.BeforeExecutionFunc is { } beforeFunc)
            await beforeFunc.Invoke();

        var (items, totalCount) = await builder.ExecutePaginationAsync(
            provider,
            requestContext.Request.SortedFields,
            requestContext.Request.Skip(),
            requestContext.Request.Take(),
            requestContext.CancellationToken);

        if (builder.AfterExecutionFunc is { } afterFunc)
            await afterFunc.Invoke();

        return new PaginationResponse<TResponse>(items, totalCount);
    }
}
