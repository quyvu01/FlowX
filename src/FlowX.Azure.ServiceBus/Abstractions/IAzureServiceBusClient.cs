using FlowX.Abstractions;

namespace FlowX.Azure.ServiceBus.Abstractions;

internal interface IAzureServiceBusClient<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext);
}