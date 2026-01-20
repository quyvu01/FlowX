using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using FlowX.Responses;
using NATS.Client.Core;

namespace FlowX.Nats.Implementations;

internal sealed class NatsClient<TRequest, TResult>(NatsClientWrapper client)
    : INatsClient<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext)
    {
        var natsHeaders = new NatsHeaders();
        requestContext.Headers?.ForEach(h => natsHeaders.Add(h.Key, h.Value));
        var messageWrapped = new MessageWrapper
            { MessageJson = JsonSerializer.Serialize(requestContext.Request) };
        var reply = await client.NatsClient
            .RequestAsync<MessageWrapper, Result<TResult>>(typeof(TRequest).GetNatsSubject(),
                messageWrapped, natsHeaders, cancellationToken: requestContext.CancellationToken);
        return reply.Data!.IsSuccess ? reply.Data.Data : throw reply.Data!.Fault.ToException();
    }
}