using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands;

public interface ICommand<out TResult> : IRequest<TResult>;

public interface ICommandVoid : ICommand<OneOf<None, Error>>;

public interface ICommandResult<TResult> : ICommand<OneOf<TResult, Error>>;