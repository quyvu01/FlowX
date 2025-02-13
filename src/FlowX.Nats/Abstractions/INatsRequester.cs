using FlowX.Abstractions;

namespace FlowX.Nats.Abstractions;

internal interface INatsRequester<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> RequestAsync(RequestContext<TRequest> requestContext);
}