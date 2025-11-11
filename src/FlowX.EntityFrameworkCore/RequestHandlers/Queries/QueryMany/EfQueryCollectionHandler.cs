using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Extensions;
using FlowX.Responses;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;

public abstract class EfQueryCollectionHandler<TModel, TQuery, TResponse>
    : IQueryHandler<TQuery, CollectionResponse<TResponse>>
    where TModel : class
    where TQuery : IQueryCollection<TResponse>
    where TResponse : class
{
    public IUnitOfWork UnitOfWork { get; } = EfCoreSharedStates.GetUnitOfWork();

    public virtual async Task<CollectionResponse<TResponse>> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var repository = UnitOfWork.RepositoryOf<TModel>();
        var buildResult = BuildQueryFlow(new QueryManyFlow<TModel, TResponse>(), requestContext);
        switch (buildResult.QuerySpecialActionType)
        {
            case QuerySpecialActionType.ToModel:
            {
                var items = await repository
                    .GetManyByConditionAsync(buildResult.Filter, db =>
                    {
                        var finalFilter = buildResult.SpecialActionToModel?.Invoke(db) ?? db;
                        return finalFilter
                            .AsNoTracking()
                            .OrderByWithDynamic(null, buildResult.SortFieldNameWhenRequestEmpty,
                                buildResult.SortedDirectionWhenRequestEmpty);
                    }, requestContext.CancellationToken);
                var itemsResponse = items.Select(a => buildResult.MapFunc.Invoke(a)).ToList();
                return new CollectionResponse<TResponse>(itemsResponse);
            }
            case QuerySpecialActionType.ToTarget:
                var srcQueryable = repository
                    .GetQueryable(buildResult.Filter)
                    .AsNoTracking();
                var queryable = srcQueryable
                    .OrderByWithDynamic(null, buildResult.SortFieldNameWhenRequestEmpty,
                        buildResult.SortedDirectionWhenRequestEmpty);
                var response = await buildResult.SpecialActionToResponse.Invoke(queryable)
                    .ToListAsync(requestContext.CancellationToken);
                return new CollectionResponse<TResponse>(response);
            case QuerySpecialActionType.UnKnown:
            default:
                throw new UnreachableException("Query special type could not be unknown!");
        }
    }

    protected abstract IQueryListFlowBuilder<TModel, TResponse> BuildQueryFlow(
        IQueryListFilter<TModel, TResponse> fromFlow, IRequestContext<TQuery> queryContext);
}