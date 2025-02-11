using FlowX.Abstractions;

namespace FlowX.Nats.Abstractions;

internal interface INatsRequester<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> RequestAsync(IRequestXContext<TRequest> requestXContext);
}