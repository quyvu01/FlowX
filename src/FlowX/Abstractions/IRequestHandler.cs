using FlowX.Abstractions.RequestFlow.Commands;

namespace FlowX.Abstractions;

public interface IRequestHandler<TRequest> where TRequest : IRequest;

public interface IRequestHandler<TRequest, TResult> where TRequest : IRequest<TResult>;