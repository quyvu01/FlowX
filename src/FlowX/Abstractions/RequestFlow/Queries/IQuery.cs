using FlowX.Errors;
using FlowX.Responses;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Queries;

public interface IQuery<out TResult> : IRequest<TResult>, IMessage;

public interface IQueryCounting : IQuery<CountingResponse>;

public interface IQueryOne<TResult> : IQuery<OneOf<TResult, ErrorDetail>>;

public interface IQueryPaged<TResult> : IQuery<PaginationResponse<TResult>> where TResult : class;

public interface IQueryCollection<TResult> : IQuery<CollectionResponse<TResult>> where TResult : class;