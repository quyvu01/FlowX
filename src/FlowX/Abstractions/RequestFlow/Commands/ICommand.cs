using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands;

public interface ICommand : IRequest, IMessage;

public interface ICommand<out TResult> : IRequest<TResult>, IMessage;

public interface ICommandVoid : ICommand<OneOf<None, ErrorDetail>>;

public interface ICommandResult<TResult> : ICommand<OneOf<TResult, ErrorDetail>> where TResult : class;