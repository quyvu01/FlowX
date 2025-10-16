using FlowX.Abstractions;
using FlowX.Nats.Abstractions;
using FlowX.Nats.ApplicationModels;
using FlowX.Nats.BackgroundServices;
using FlowX.Nats.Implementations;
using FlowX.Nats.Statics;
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
        flowXRegister.ServiceCollection.AddSingleton(_ => new NatsClientWrapper(new NatsClient(NatsStatics.NatsUrl)));
        ClientsRegister(flowXRegister.ServiceCollection);
        flowXRegister.ServiceCollection.AddScoped<ITransportService, NatsTransportService>();
        
        flowXRegister.ServiceCollection.AddSingleton(typeof(INatsServer<,>), typeof(NatsServer<,>));
        flowXRegister.ServiceCollection.AddHostedService<NatsServerWorker>();
    }

    private static void ClientsRegister(IServiceCollection serviceCollection) =>
        serviceCollection.AddScoped(typeof(INatsClient<,>), typeof(NatsClient<,>));
}