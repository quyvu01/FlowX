using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Cached;
using FlowX.Exceptions;
using FlowX.Extensions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using FlowX.Structs;
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
            .RequestAsync<TRequest, NatMessageWrapper>(typeof(TRequest).GetNatsSubject(),
                requestContext!.Request, natsHeaders, cancellationToken: requestContext.CancellationToken);
        if (reply.Data is not { } data)
            throw new ArgumentNullException(nameof(reply.Data));
        var responseType = Type.GetType(data.AssemblyType);
        if (responseType is null)
            throw new FlowXExceptions.NoMatchingTypeForResponse(data.AssemblyType);
        var response = JsonSerializer.Deserialize(data.MessageAsString, responseType);
        var matchResponseType = FlowXCached.RequestMapResponse.Value
            .TryGetValue(typeof(TRequest), out var matchedResponseType);
        if (!matchResponseType)
            throw new FlowXExceptions.NoMatchingTypeForResponse(data.AssemblyType);
        if (matchedResponseType.IsGenericType && matchedResponseType.GetGenericTypeDefinition() == typeof(OneOf<,>))
        {
            
        }

        if (response is not TResult result)
            throw new FlowXExceptions.NoMatchingTypeForResponse(data.AssemblyType);
        return result;
    }
}