using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.RabbitMq.Abstractions;
using FlowX.RabbitMq.ApplicationModels;
using FlowX.RabbitMq.BackgroundServices;
using FlowX.RabbitMq.Implementations;
using FlowX.RabbitMq.InternalPipelines;
using FlowX.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.RabbitMq.Extensions;

public static class RabbitMqExtensions
{
    public static void AddRabbitMq(this FlowXRegister flowXRegister, Action<RabbitMqConfigurator> options)
    {
        var config = new RabbitMqConfigurator();
        options.Invoke(config);
        var services = flowXRegister.ServiceCollection;
        services.AddSingleton<IRabbitMqServer, RabbitMqServer>();
        services.AddSingleton<IRequestClient, RabbitMqRequestClient>();
        services.AddHostedService<RabbitMqServerHostedService>();
        services.AddScoped<IInternalHeaderInjector, InternalHeaderInjector>();
        services.AddScoped<ITransportService, RabbitMqTransportService>();
        flowXRegister.AddPipelines(c => c.OfType(typeof(InternalHeadersPipelineBehavior<,>)));
    }
}