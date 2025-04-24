using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.Messages;
using FlowX.Nats.Abstractions;
using FlowX.Nats.Extensions;
using FlowX.Nats.Wrappers;
using FlowX.Structs;
using NATS.Client.Core;

namespace FlowX.Nats.Implementations;

internal sealed class NatsRequester<TRequest, TResult>(NatsClientWrapper client)
    : INatsRequester<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private static readonly Func<MessageSerialized, TResult> OneOfFactory = CreateOneOfFactory();

    public async Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext)
    {
        var natsHeaders = new NatsHeaders();
        requestContext.Headers?.ForEach(h => natsHeaders.Add(h.Key, h.Value));
        var natsMessageWrapped = new NatsMessageWrapper
            { MessageAsString = JsonSerializer.Serialize(requestContext.Request) };
        var reply = await client.NatsClient
            .RequestAsync<NatsMessageWrapper, MessageSerialized>(typeof(TRequest).GetNatsSubject(),
                natsMessageWrapped, natsHeaders, cancellationToken: requestContext.CancellationToken);
        if (reply.Data is not { } data) throw new ArgumentNullException(nameof(reply.Data));
        if (!typeof(IOneOf).IsAssignableFrom(typeof(TResult)))
            return (TResult)JsonSerializer.Deserialize(data.ObjectSerialized, Type.GetType(data.Type)!);
        return OneOfFactory!(data);
    }

    private static Func<MessageSerialized, TResult> CreateOneOfFactory()
    {
        var param = Expression.Parameter(typeof(MessageSerialized), "message");
        var method = typeof(TResult).GetMethod("DeSerialize", BindingFlags.Public | BindingFlags.Static);
        var call = Expression.Call(method!, param);
        var lambda = Expression.Lambda<Func<MessageSerialized, TResult>>(call, param);
        return lambda.Compile();
    }
}