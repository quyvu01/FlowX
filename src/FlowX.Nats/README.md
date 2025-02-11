# FlowX.Nats

FlowX.Nats is an extension package for FlowX that integrates with NATS.io, providing a seamless way to handle
message-based communication using FlowX’s Request Flow patterns. This package is ideal for distributed applications that
require reliable event-driven architecture.

[Demo Project](https://github.com/quyvu01/FlowXDemo)

## Features

- Enables communication via NATS in FlowX.
- Supports request-response and event-driven patterns.
- Leverages FlowX’s fluent API for seamless integration.

## Prerequisites

### Prerequisites

Ensure you have the following installed:

- .NET 8.0 or higher

### Installation

To install the FlowX package, use the following NuGet command:

```bash
dotnet add package FlowX.Nats
```

Or via the NuGet Package Manager:

```bash
Install-Package FlowX.Nats
```

## Usage

### 1. Register FlowX.Nats in the Dependency Injection Container

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
    options.AddNats(config => config.Url("nats://localhost:4222"));
});
```

#### Method descriptions

`AddNats`: Add Nats config, you have to locate the nats `Url`, you can also config the Prefix for Nats.

### 2. Sending Requests via NATS

```csharp
var sender = ServiceProvider.GetRequiredService<IFlowXSender>();
var response = await sender.Send(new GetProvince("1"));
```

Acts as `FlowX`, you don't need to care about the behind the scene. FlowX handle the RPC request for you!

Enjoy your moment!
---

| Package Name                                                                           | Description                                                                                         | .NET Version | Document                                                                                     |
|----------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------|--------------|----------------------------------------------------------------------------------------------|
| [FlowX](https://www.nuget.org/packages/FlowX/)                                         | FlowX core                                                                                          | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.EntityFrameworkCore/README.md) |
| **Data Provider**                                                                      |                                                                                                     |
| [FlowX.EntityFrameworkCore](https://www.nuget.org/packages/FlowX.EntityFrameworkCore/) | This is the FlowX extension package using EntityFramework to fetch data                             | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.EntityFrameworkCore/README.md) |
| **Transports**                                                                         |                                                                                                     |
| [FlowX.Nats](https://www.nuget.org/packages/FlowX.Nats/)                               | FlowX.Nats is an extension package for FlowX that leverages Nats for efficient data transportation. | 8.0, 9.0     | This Document                                                                                |

