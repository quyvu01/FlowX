namespace FlowX.Abstractions.RequestFlow.Commands;

public interface ICommandHandler<TRequest> : IRequestHandler<TRequest> where TRequest : ICommand;

public interface ICommandHandler<TRequest, TResult> : IRequestHandler<TRequest, TResult>, IMessageHandler
    where TRequest : ICommand<TResult>;