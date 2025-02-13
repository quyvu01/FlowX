using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.Extensions;
using FlowX.Responses;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;

public abstract class EfQueryPaginationHandler<TModel, TQuery, TResponse>(
    ISqlRepository<TModel> sqlRepository)
    : IQueryHandler<TQuery, PaginationResponse<TResponse>>
    where TModel : class
    where TQuery : GetManyQuery, IQueryPaged<TResponse>
    where TResponse : class
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;

    protected abstract IQueryListFlowBuilder<TModel, TResponse> BuildQueryFlow(
        IQueryListFilter<TModel, TResponse> fromFlow, RequestContext<TQuery> queryContext);

    public virtual async Task<PaginationResponse<TResponse>> HandleAsync(RequestContext<TQuery> requestContext)
    {
        var buildResult = BuildQueryFlow(new QueryManyFlow<TModel, TResponse>(), requestContext);
        switch (buildResult.QuerySpecialActionType)
        {
            case QuerySpecialActionType.ToModel:
            {
                var toModelSrcQueryable = SqlRepository
                    .GetQueryable(buildResult.Filter)
                    .AsNoTracking();
                var toModelQueryable = toModelSrcQueryable
                    .OrderByWithDynamic(requestContext.Request.SortedFieldName,
                        buildResult.SortFieldNameWhenRequestEmpty,
                        requestContext.Request.SortedDirection ?? buildResult.SortedDirectionWhenRequestEmpty);
                var toModelFinalQueryable = buildResult.SpecialActionToModel.Invoke(toModelQueryable);
                var toModelResponse = await toModelFinalQueryable
                    .Offset(requestContext.Request.Skip)
                    .Limit(requestContext.Request.Take)
                    .ToListAsync(requestContext.CancellationToken);
                var toModelTotalRecord = await toModelFinalQueryable.LongCountAsync(requestContext.CancellationToken);
                var itemsResponses = toModelResponse.Select(a => buildResult.MapFunc.Invoke(a)).ToList();
                return new PaginationResponse<TResponse>(itemsResponses, toModelTotalRecord);
            }
            case QuerySpecialActionType.ToTarget:
                var srcQueryable = SqlRepository
                    .GetQueryable(buildResult.Filter)
                    .AsNoTracking();

                var queryable = srcQueryable
                    .OrderByWithDynamic(requestContext.Request.SortedFieldName,
                        buildResult.SortFieldNameWhenRequestEmpty,
                        requestContext.Request.SortedDirection ?? buildResult.SortedDirectionWhenRequestEmpty);
                var finalQueryable = buildResult.SpecialActionToResponse.Invoke(queryable);
                var response = await finalQueryable
                    .Offset(requestContext.Request.Skip)
                    .Limit(requestContext.Request.Take)
                    .ToListAsync(requestContext.CancellationToken);
                var totalRecord = await finalQueryable.LongCountAsync(requestContext.CancellationToken);
                return new PaginationResponse<TResponse>(response, totalRecord);
            case QuerySpecialActionType.UnKnown:
            default:
                throw new UnreachableException("Query special type could not be unknown!");
        }
    }
}