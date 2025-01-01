namespace FlowX.Abstractions.RequestFlow.Commands;

public interface ICommandHandler<in TRequest, TResult> : IRequestHandler<TRequest, TResult>, IMessageHandler
    where TRequest : ICommand<TResult>;