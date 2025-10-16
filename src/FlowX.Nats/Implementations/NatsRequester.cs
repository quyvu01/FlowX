using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using NATS.Client.Core;

namespace FlowX.Nats.Implementations;

internal sealed class NatsRequester<TRequest, TResult>(NatsClientWrapper client)
    : INatsRequester<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext)
    {
        var natsHeaders = new NatsHeaders();
        requestContext.Headers?.ForEach(h => natsHeaders.Add(h.Key, h.Value));
        var natsMessageWrapped = new NatsMessageWrapper
            { MessageAsString = JsonSerializer.Serialize(requestContext.Request) };
        var reply = await client.NatsClient
            .RequestAsync<NatsMessageWrapper, TResult>(typeof(TRequest).GetNatsSubject(),
                natsMessageWrapped, natsHeaders, cancellationToken: requestContext.CancellationToken);
        return reply.Data;
    }
}