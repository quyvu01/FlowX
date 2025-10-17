using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FlowX.Azure.ServiceBus.Wrappers;

internal record AzureServiceBusClientWrapper(
    ServiceBusClient ServiceBusClient,
    ServiceBusAdministrationClient ServiceBusAdministrationClient);