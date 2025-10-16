using FlowX.Abstractions;

namespace FlowX.Nats.Abstractions;

internal interface INatsClient<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext);
}