using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;

public abstract class EfQueryOneHandler<TModel, TQuery, TResponse>
    : IQueryHandler<TQuery, TResponse>
    where TModel : class
    where TQuery : class, IQueryOne<TResponse>
    where TResponse : class
{
    protected abstract IQueryOneFlowBuilder<TModel, TResponse> BuildQueryFlow(
        IQueryOneFilter<TModel, TResponse> fromFlow, IRequestContext<TQuery> queryContext);

    public virtual async Task<TResponse> HandleAsync(IRequestContext<TQuery> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var repository = unitOfWork.RepositoryOf<TModel>();
        var buildResult = BuildQueryFlow(new QueryOneFlow<TModel, TResponse>(), requestContext);
        switch (buildResult.QuerySpecialActionType)
        {
            case QuerySpecialActionType.UnKnown:
                throw new UnreachableException("Query special type could not be unknown!");
            case QuerySpecialActionType.ToModel:
            {
                var item = await repository.GetFirstByConditionAsync(buildResult.Filter,
                    db => buildResult.SpecialAction?
                        .Invoke(db.AsNoTracking()) ?? db.AsNoTracking(), requestContext.CancellationToken);
                return item is null ? throw buildResult.Error : buildResult.MapFunc.Invoke(item);
            }
            case QuerySpecialActionType.ToTarget:
            default:
            {
                var collection = repository.GetQueryable(buildResult.Filter)
                    .AsNoTracking();
                var item = await buildResult.SpecialActionToResponse.Invoke(collection)
                    .FirstOrDefaultAsync(requestContext.CancellationToken);
                return item ?? throw buildResult.Error;
            }
        }
    }
}