using FlowX.Abstractions;

namespace FlowX.Nats.Abstractions;

public interface INatsServerRpc
{
    Task SubscribeAsync();
}

public interface INatsServerRpc<TRequest, TResult> : INatsServerRpc where TRequest : IRequest<TResult>;