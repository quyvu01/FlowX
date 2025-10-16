using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;

public abstract class EfQueryOneHandler<TModel, TQuery, TResponse>(
    ISqlRepository<TModel> sqlRepository)
    : IQueryHandler<TQuery, TResponse>
    where TModel : class
    where TQuery : class, IQueryOne<TResponse>
    where TResponse : class
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;

    protected abstract IQueryOneFlowBuilder<TModel, TResponse> BuildQueryFlow(
        IQueryOneFilter<TModel, TResponse> fromFlow, IRequestContext<TQuery> queryContext);

    public virtual async Task<TResponse> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var buildResult = BuildQueryFlow(new QueryOneFlow<TModel, TResponse>(), requestContext);
        switch (buildResult.QuerySpecialActionType)
        {
            case QuerySpecialActionType.UnKnown:
                throw new UnreachableException("Query special type could not be unknown!");
            case QuerySpecialActionType.ToModel:
            {
                var item = await SqlRepository.GetFirstByConditionAsync(buildResult.Filter,
                    db => buildResult.SpecialAction?
                        .Invoke(db.AsNoTracking()) ?? db.AsNoTracking(), requestContext.CancellationToken);
                return item is null ? throw buildResult.Error : buildResult.MapFunc.Invoke(item);
            }
            case QuerySpecialActionType.ToTarget:
            default:
            {
                var collection = SqlRepository.GetQueryable(buildResult.Filter)
                    .AsNoTracking();
                var item = await buildResult.SpecialActionToResponse.Invoke(collection)
                    .FirstOrDefaultAsync(requestContext.CancellationToken);
                return item ?? throw buildResult.Error;
            }
        }
    }
}