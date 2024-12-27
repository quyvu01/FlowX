namespace FlowX.Abstractions.RequestFlow.Queries;

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult>, IMessageHandler
    where TQuery : IQuery<TResult>;