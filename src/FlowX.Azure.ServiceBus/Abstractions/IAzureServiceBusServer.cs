using FlowX.Abstractions;

namespace FlowX.Azure.ServiceBus.Abstractions;

internal interface IAzureServiceBusServer
{
    Task StartAsync();
}

internal interface IAzureServiceBusServer<TRequest, TResult> : IAzureServiceBusServer
    where TRequest : IRequest<TResult>;