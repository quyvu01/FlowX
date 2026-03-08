using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryPipeline;

public abstract class EfQueryPipelineCollectionHandler<TQuery, TResponse>
    : EfQueryPipelineHandler<TQuery, CollectionResponse<TResponse>>
    where TQuery : class, IQueryCollection<TResponse>
    where TResponse : class;