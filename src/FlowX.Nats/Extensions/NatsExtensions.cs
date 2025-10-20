using FlowX.Abstractions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.ApplicationModels;
using FlowX.Nats.BackgroundServices;
using FlowX.Nats.Implementations;
using FlowX.Nats.Wrappers;
using FlowX.Registries;
using Microsoft.Extensions.DependencyInjection;
using NATS.Net;

namespace FlowX.Nats.Extensions;

public static class NatsExtensions
{
    public static void AddNats(this FlowXRegister flowXRegister, Action<NatsClientHost> options)
    {
        var newClientsRegister = new NatsClientHost();
        options.Invoke(newClientsRegister);
        var opts = newClientsRegister.NatsOptions;
        var natsUrl = newClientsRegister.NatsUrl;
        flowXRegister.ServiceCollection.AddSingleton(_ => opts != null
            ? new NatsClientWrapper(new NatsClient(opts))
            : new NatsClientWrapper(new NatsClient(natsUrl)));

        ClientsRegister(flowXRegister.ServiceCollection);
        flowXRegister.ServiceCollection.AddScoped<ITransportService, NatsTransportService>();

        flowXRegister.ServiceCollection.AddSingleton(typeof(INatsServer<,>), typeof(NatsServer<,>));
        flowXRegister.ServiceCollection.AddHostedService<NatsServerWorker>();
    }

    private static void ClientsRegister(IServiceCollection serviceCollection) =>
        serviceCollection.AddScoped(typeof(INatsClient<,>), typeof(NatsClient<,>));
}