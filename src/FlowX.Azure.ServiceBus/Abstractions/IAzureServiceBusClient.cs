using FlowX.Abstractions;

namespace FlowX.Azure.ServiceBus.Abstractions;

internal interface IAzureServiceBusClient<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext);
}

internal interface IAzureServiceBusClient<in TRequest> where TRequest : IRequest
{
    Task RequestAsync(IRequestContext<TRequest> requestContext);
}