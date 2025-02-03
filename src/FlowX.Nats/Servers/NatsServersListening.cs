using FlowX.Cached;
using FlowX.Extensions;
using FlowX.Nats.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Nats.Servers;

internal static class NatsServersListening
{
    internal static void StartAsync(IServiceProvider serviceProvider)
    {
        var requestMapResponseTypes = FlowXCached.RequestMapResponse.Value;
        requestMapResponseTypes.ForEach(requestMapResponseType => Task.Factory.StartNew(async () =>
        {
            var requestType = requestMapResponseType.Key;
            var responseType = requestMapResponseType.Value;
            var service = serviceProvider.GetRequiredService(typeof(INatsServerRpc<,>)
                .MakeGenericType(requestType, responseType));
            if (service is INatsServerRpc natsServerRpc)
                await natsServerRpc.SubscribeAsync();
        }));
    }
}