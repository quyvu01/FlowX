using FlowX.Abstractions;

namespace FlowX.Implementations;

internal sealed class DefaultRequestHandler<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public Task<TResult> HandleAsync(RequestContext<TRequest> requestXContext) => Task.FromResult((TResult)default);
}