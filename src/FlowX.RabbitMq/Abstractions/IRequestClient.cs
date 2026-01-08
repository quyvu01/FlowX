using FlowX.Abstractions;

namespace FlowX.RabbitMq.Abstractions;

public interface IRequestClient
{
    Task<TResult> RequestAsync<TRequest, TResult>(IRequestContext<TRequest> requestContext)
        where TRequest : IRequest<TResult>;
}