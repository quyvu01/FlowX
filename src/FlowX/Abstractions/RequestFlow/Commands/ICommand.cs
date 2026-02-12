namespace FlowX.Abstractions.RequestFlow.Commands;

public interface ICommand<out TResult> : IRequest<TResult>;

public interface ICommand : IRequest;

public interface ICommandVoid : ICommand;

public interface ICommandResult<out TResult> : ICommand<TResult>;