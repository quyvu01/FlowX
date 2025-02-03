using FlowX.Abstractions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.ApplicationModels;
using FlowX.Nats.Implementations;
using FlowX.Nats.Servers;
using FlowX.Nats.Statics;
using FlowX.Nats.Wrappers;
using FlowX.Registries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Net;

namespace FlowX.Nats.Extensions;

public static class NatsExtensions
{
    public static void AddNats(this FlowXRegister flowXRegister, Action<NatsClientHost> options)
    {
        var newClientsRegister = new NatsClientHost();
        options.Invoke(newClientsRegister);

        flowXRegister.ServiceCollection.AddSingleton(_ =>
        {
            var client = new NatsClient(NatsStatics.NatsUrl);
            return new NatsClientWrapper(client);
        });
        ClientsRegister(flowXRegister.ServiceCollection);
        flowXRegister.ServiceCollection.AddTransient(typeof(INatsServerRpc<,>), typeof(NatsServerRpc<,>));
        flowXRegister.ServiceCollection.AddTransient<ITransportService, NatsTransportService>();
    }

    private static void ClientsRegister(IServiceCollection serviceCollection) =>
        serviceCollection.AddScoped(typeof(INatsRequester<,>), typeof(NatsRequester<,>));

    public static void StartNatsListeningAsync(this IHost host) =>
        NatsServersListening.StartAsync(host.Services);
}