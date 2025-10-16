using FlowX.Responses;

namespace FlowX.Abstractions.RequestFlow.Queries;

public interface IQuery<out TResult> : IRequest<TResult>;

public interface IQueryCounting : IQuery<CountingResponse>;

public interface IQueryOne<out TResult> : IQuery<TResult>;

public interface IQueryPaged<TResult> : IQuery<PaginationResponse<TResult>> where TResult : class;

public interface IQueryCollection<TResult> : IQuery<CollectionResponse<TResult>> where TResult : class;