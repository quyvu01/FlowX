# FlowX.Azure.ServiceBus

FlowX.Azure.ServiceBus is an extension package for FlowX that integrates with Azure ServiceBus, providing a seamless way
to handle
message-based communication using FlowX’s Request Flow patterns. This package is ideal for distributed applications that
require reliable event-driven architecture.

[Demo Project](https://github.com/quyvu01/FlowXDemo)

## Features

- Enables communication via Azure ServiceBus in FlowX.
- Supports request-response and event-driven patterns.
- Leverages FlowX’s fluent API for seamless integration.

## Prerequisites

### Prerequisites

Ensure you have the following installed:

- .NET 8.0 or higher
- Azure.Messaging.ServiceBus

### Installation

To install the FlowX package, use the following NuGet command:

```bash
dotnet add package FlowX.Azure.ServiceBus
```

Or via the NuGet Package Manager:

```bash
Install-Package FlowX.Azure.ServiceBus
```

## Usage

### 1. Register FlowX.Azure.ServiceBus in the Dependency Injection Container

Add FlowX.Nats to your service configuration:

```csharp
builder.Services.AddFlowX(options =>
{
    options.AddModelsFromNamespaceContaining<IAssemblyMarker>();
    options.AddHandlersFromNamespaceContaining<IAssemblyMarker>();
    options.AddDbContextDynamic<Service1DbContext>(opts =>
    {
        opts.AddDynamicRepositories();
        opts.AddDynamicUnitOfWork();
    });
options.AddAzureServiceBus(cfg => cfg.Host("SensitiveConnectionString"));
});
```

#### Method descriptions

`AddAzureServiceBus`: Add AddAzureServiceBus config, you have to locate the host `Url`, you can also config the Prefix
for AzureServiceBus.

### 2. Sending Requests via Azure ServiceBus

```csharp
var sender = ServiceProvider.GetRequiredService<IFlow>();
var response = await sender.Send(new GetProvinces());
```

Acts as `FlowX`, you don't need to care about the behind the scene. FlowX handle the RPC request for you!

Enjoy your moment!
---

| Package Name                                                 | Description                                                                                                                 | .NET Version | Document                                                                                     |
|--------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------|--------------|----------------------------------------------------------------------------------------------|
| [FlowX][FlowX.nuget]                                         | FlowX core                                                                                                                  | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/README.md)                               |
| **Data Provider**                                            |                                                                                                                             |
| [FlowX.EntityFrameworkCore][FlowX.EntityFrameworkCore.nuget] | This is the FlowX extension package using EntityFramework to fetch data                                                     | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.EntityFrameworkCore/README.md) |
| **Transports**                                               |                                                                                                                             |
| [FlowX.Nats][FlowX.Nats.nuget]                               | FlowX.Nats is an extension package for FlowX that leverages Nats for efficient data transportation.                         | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.Nats/README.md)                |
| [FlowX.Azure.ServiceBus][FlowX.Azure.ServiceBus.nuget]       | FlowX.Azure.ServiceBus is an extension package for FlowX that leverages Azure ServiceBus for efficient data transportation. | 8.0, 9.0     | This Document                                                                                |


[FlowX.nuget]: https://www.nuget.org/packages/FlowX/

[FlowX.EntityFrameworkCore.nuget]: https://www.nuget.org/packages/FlowX.EntityFrameworkCore/

[FlowX.Nats.nuget]: https://www.nuget.org/packages/FlowX.Nats/

[FlowX.Azure.ServiceBus.nuget]: https://www.nuget.org/packages/FlowX.Azure.ServiceBus/