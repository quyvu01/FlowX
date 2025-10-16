using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands;

public interface ICommand<out TResult> : IRequest<TResult>;
public interface ICommandVoid : ICommand<None>;
public interface ICommandResult<out TResult> : ICommand<TResult>;