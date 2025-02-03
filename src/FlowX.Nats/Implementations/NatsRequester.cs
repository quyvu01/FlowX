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
    public async Task<TResult> RequestAsync(RequestContext<TRequest> requestContext)
    {
        var natsHeaders = new NatsHeaders();
        requestContext.Headers?.ForEach(h => natsHeaders.Add(h.Key, h.Value));
        var reply = await client.NatsClient
            .RequestAsync<TRequest, dynamic>(typeof(TRequest).GetNatsSubject(),
                requestContext!.Request, natsHeaders, cancellationToken: requestContext.CancellationToken);
        if (reply.Data is not { } data)
            throw new ArgumentNullException(nameof(reply.Data));
        return data;
    }
}