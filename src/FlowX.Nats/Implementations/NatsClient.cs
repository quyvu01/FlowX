using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using FlowX.Wrappers;
using NATS.Client.Core;

namespace FlowX.Nats.Implementations;

internal sealed class NatsClient<TRequest, TResult>(NatsClientWrapper client)
    : INatsClient<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext)
    {
        var natsHeaders = new NatsHeaders();
        requestContext.Headers?.ForEach(h => natsHeaders.Add(h.Key, h.Value));
        var natsMessageWrapped = new NatsMessageWrapper
            { MessageAsString = JsonSerializer.Serialize(requestContext.Request) };
        var reply = await client.NatsClient
            .RequestAsync<NatsMessageWrapper, NatResponseWrapped<TResult>>(typeof(TRequest).GetNatsSubject(),
                natsMessageWrapped, natsHeaders, cancellationToken: requestContext.CancellationToken);
        var result = reply.Data;
        if (result is null) throw new ArgumentNullException(nameof(result));
        return result.TypeAssembly is null
            ? result.Response
            : throw ExceptionSerializableWrapper.ToException(result.ExceptionSerializable);
    }
}