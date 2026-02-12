namespace FlowX.Abstractions.RequestFlow.Commands;

public interface ICommandHandler<in TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>;
    
public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest>
    where TRequest : ICommand;