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
        IQueryListFilter<TModel, TResponse> fromFlow, IRequestXContext<TQuery> queryXContext);

    public virtual async Task<PaginationResponse<TResponse>> HandleAsync(IRequestXContext<TQuery> requestXContext)
    {
        var buildResult = BuildQueryFlow(new QueryManyFlow<TModel, TResponse>(), requestXContext);
        switch (buildResult.QuerySpecialActionType)
        {
            case QuerySpecialActionType.ToModel:
            {
                var toModelSrcQueryable = SqlRepository
                    .GetQueryable(buildResult.Filter)
                    .AsNoTracking();
                var toModelQueryable = toModelSrcQueryable
                    .OrderByWithDynamic(requestXContext.Request.SortedFieldName,
                        buildResult.SortFieldNameWhenRequestEmpty,
                        requestXContext.Request.SortedDirection ?? buildResult.SortedDirectionWhenRequestEmpty);
                var toModelFinalQueryable = buildResult.SpecialActionToModel.Invoke(toModelQueryable);
                var toModelResponse = await toModelFinalQueryable
                    .Offset(requestXContext.Request.Skip)
                    .Limit(requestXContext.Request.Take)
                    .ToListAsync(requestXContext.CancellationToken);
                var toModelTotalRecord = await toModelFinalQueryable.LongCountAsync(requestXContext.CancellationToken);
                var itemsResponses = toModelResponse.Select(a => buildResult.MapFunc.Invoke(a)).ToList();
                return new PaginationResponse<TResponse>(itemsResponses, toModelTotalRecord);
            }
            case QuerySpecialActionType.ToTarget:
                var srcQueryable = SqlRepository
                    .GetQueryable(buildResult.Filter)
                    .AsNoTracking();

                var queryable = srcQueryable
                    .OrderByWithDynamic(requestXContext.Request.SortedFieldName,
                        buildResult.SortFieldNameWhenRequestEmpty,
                        requestXContext.Request.SortedDirection ?? buildResult.SortedDirectionWhenRequestEmpty);
                var finalQueryable = buildResult.SpecialActionToResponse.Invoke(queryable);
                var response = await finalQueryable
                    .Offset(requestXContext.Request.Skip)
                    .Limit(requestXContext.Request.Take)
                    .ToListAsync(requestXContext.CancellationToken);
                var totalRecord = await finalQueryable.LongCountAsync(requestXContext.CancellationToken);
                return new PaginationResponse<TResponse>(response, totalRecord);
            case QuerySpecialActionType.UnKnown:
            default:
                throw new UnreachableException("Query special type could not be unknown!");
        }
    }
}