using FlowX.Abstractions;

namespace FlowX.Nats.Abstractions;

public interface INatsServer
{
    Task SubscribeAsync();
}

public interface INatsServer<TRequest, TResult> : INatsServer where TRequest : IRequest<TResult>;