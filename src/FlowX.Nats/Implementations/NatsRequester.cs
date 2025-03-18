using System.Linq.Expressions;
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
    private static readonly Func<object, TResult> OneOfFactory = CreateOneOfFactory();

    public async Task<TResult> RequestAsync(IRequestContext<TRequest> requestContext)
    {
        var natsHeaders = new NatsHeaders();
        requestContext.Headers?.ForEach(h => natsHeaders.Add(h.Key, h.Value));
        var natsMessageWrapped = new NatsMessageWrapper
            { MessageAsString = JsonSerializer.Serialize(requestContext.Request) };
        var reply = await client.NatsClient
            .RequestAsync<NatsMessageWrapper, MessageSerialized>(typeof(TRequest).GetNatsSubject(),
                natsMessageWrapped, natsHeaders, cancellationToken: requestContext.CancellationToken);
        if (reply.Data is not { } data)
            throw new ArgumentNullException(nameof(reply.Data));
        var response = JsonSerializer.Deserialize(data.ObjectSerialized, Type.GetType(data.Type)!);
        if (!typeof(IOneOf).IsAssignableFrom(typeof(TResult))) return (TResult)response;
        return OneOfFactory!(response);
    }

    private static Func<object, TResult> CreateOneOfFactory()
    {
        var param = Expression.Parameter(typeof(object), "arg");
        var convertedParam =
            Expression.Convert(param, typeof(TResult).GetGenericArguments()[0]); // Convert to the underlying type
        var newExpr = Expression.New(typeof(TResult).GetConstructor([convertedParam.Type])!, convertedParam);
        var lambda = Expression.Lambda<Func<object, TResult>>(newExpr, param);
        return lambda.Compile();
    }
}