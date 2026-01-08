# FlowX.RabbitMq

FlowX.RabbitMq is an extension package for FlowX that integrates with RabbitMq, providing a seamless way to handle
message-based communication using FlowX’s Request Flow patterns. This package is ideal for distributed applications that
require reliable event-driven architecture.

[Demo Project](https://github.com/quyvu01/FlowXDemo)

## Features

- Enables communication via RabbitMq in FlowX.
- Supports request-response and event-driven patterns.
- Leverages FlowX’s fluent API for seamless integration.

## Prerequisites

### Prerequisites

Ensure you have the following installed:

- .NET 8.0 or higher
- RabbitMQ.Client

### Installation

To install the FlowX package, use the following NuGet command:

```bash
dotnet add package FlowX.RabbitMq
```

Or via the NuGet Package Manager:

```bash
Install-Package FlowX.RabbitMq
```

## Usage

### 1. Register FlowX.RabbitMq in the Dependency Injection Container

Add FlowX.RabbitMq to your service configuration:

```csharp
builder.Services.AddFlowX(options =>
{
    options.AddModelsFromNamespaceContaining<IAssemblyMarker>();
    options.AddHandlersFromNamespaceContaining<IAssemblyMarker>();
    options.AddRabbitMq(c => c.Host("localhost", "/"));
})
.AddEfCore(c => c.AddDbContexts(typeof(SomeDbContext)));
```

#### Method descriptions

`AddRabbitMq`: Add RabbitMq config, you have to locate the rabbitMq `Host` and `VirtualHost`, you can also config the Prefix for RabbitMq.

### 2. Sending Requests via NATS

```csharp
var sender = ServiceProvider.GetRequiredService<IMediator>();
var response = await sender.Send(new GetProvinces(), cancellationToken);
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
| [FlowX.Nats][FlowX.Nats.nuget]                               | FlowX.Nats is an extension package for FlowX that leverages Nats for efficient data transportation.                         | 8.0, 9.0     | This Document                                                                                |
| [FlowX.Azure.ServiceBus][FlowX.Azure.ServiceBus.nuget]       | FlowX.Azure.ServiceBus is an extension package for FlowX that leverages Azure ServiceBus for efficient data transportation. | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.Azure.ServiceBus/README.md)    |

[FlowX.nuget]: https://www.nuget.org/packages/FlowX/

[FlowX.EntityFrameworkCore.nuget]: https://www.nuget.org/packages/FlowX.EntityFrameworkCore/

[FlowX.Nats.nuget]: https://www.nuget.org/packages/FlowX.Nats/

[FlowX.Azure.ServiceBus.nuget]: https://www.nuget.org/packages/FlowX.Azure.ServiceBus/