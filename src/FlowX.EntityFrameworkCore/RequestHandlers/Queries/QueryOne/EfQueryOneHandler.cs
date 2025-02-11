using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.Errors;
using FlowX.Structs;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;

public abstract class EfQueryOneHandler<TModel, TQuery, TResponse>(
    ISqlRepository<TModel> sqlRepository)
    : IQueryHandler<TQuery, OneOf<TResponse, Error>>
    where TModel : class
    where TQuery : class, IQueryOne<TResponse>
    where TResponse : class
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;

    protected abstract IQueryOneFlowBuilder<TModel, TResponse> BuildQueryFlow(
        IQueryOneFilter<TModel, TResponse> fromFlow, IRequestXContext<TQuery> queryXContext);

    public virtual async Task<OneOf<TResponse, Error>> HandleAsync(IRequestXContext<TQuery> requestXContext)
    {
        var buildResult = BuildQueryFlow(new QueryOneFlow<TModel, TResponse>(), requestXContext);
        switch (buildResult.QuerySpecialActionType)
        {
            case QuerySpecialActionType.UnKnown:
                throw new UnreachableException("Query special type could not be unknown!");
            case QuerySpecialActionType.ToModel:
            {
                var item = await SqlRepository.GetFirstByConditionAsync(buildResult.Filter,
                    db => buildResult.SpecialAction?
                        .Invoke(db.AsNoTracking()) ?? db.AsNoTracking(), requestXContext.CancellationToken);
                if (item is null) return buildResult.Error;
                return buildResult.MapFunc.Invoke(item);
            }
            case QuerySpecialActionType.ToTarget:
            default:
            {
                var collection = SqlRepository.GetQueryable(buildResult.Filter)
                    .AsNoTracking();
                return await buildResult.SpecialActionToResponse.Invoke(collection)
                    .FirstOrDefaultAsync(requestXContext.CancellationToken);
            }
        }
    }
}