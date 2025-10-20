using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FlowX.Abstractions;
using FlowX.Azure.ServiceBus.Abstractions;
using FlowX.Azure.ServiceBus.ApplicationModels;
using FlowX.Azure.ServiceBus.BackgroundServices;
using FlowX.Azure.ServiceBus.Implementations;
using FlowX.Azure.ServiceBus.Wrappers;
using FlowX.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Azure.ServiceBus.Extensions;

public static class AzureServiceBusExtensions
{
    public static void AddAzureServiceBus(this FlowXRegister flowXRegister,
        Action<AzureServiceBusClientSetting> options)
    {
        var setting = new AzureServiceBusClientSetting();
        options.Invoke(setting);
        var connectionString = setting.ConnectionString;
        var serviceBusClientOptions = setting.ServiceBusClientOptions;
        var client = new ServiceBusClient(connectionString, serviceBusClientOptions);
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        var clientWrapper = new AzureServiceBusClientWrapper(client, adminClient);

        flowXRegister.ServiceCollection.AddSingleton(clientWrapper);
        flowXRegister.ServiceCollection.AddScoped<ITransportService, AzureServiceBusTransportService>();
        flowXRegister.ServiceCollection.AddSingleton(typeof(IAzureServiceBusServer<,>),
            typeof(AzureServiceBusServer<,>));
        flowXRegister.ServiceCollection.AddHostedService<AzureServiceBusServerWorker>();
        flowXRegister.ServiceCollection.AddSingleton(typeof(IAzureServiceBusClient<,>),
            typeof(AzureServiceBusClient<,>));
    }
}