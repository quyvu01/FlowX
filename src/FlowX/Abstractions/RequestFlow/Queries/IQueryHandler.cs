namespace FlowX.Abstractions.RequestFlow.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>, IMessageHandler
    where TQuery : IQuery<TResult>;