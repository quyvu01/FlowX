using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Responses;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryPipeline;

public abstract class EfQueryPipelinePaginatedHandler<TQuery, TResponse>
    : EfQueryPipelineHandler<TQuery, PaginationResponse<TResponse>>
    where TQuery : class, IQueryPaged<TResponse>
    where TResponse : class;