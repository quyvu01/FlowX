# FlowX

FlowX is a modern library designed to simplify the creation and management of Request Flow patterns in .NET
applications. This library is ideal for developers building enterprise-grade solutions who need a seamless approach to
implementing CQRS (Command Query Responsibility Segregation) patterns.

[Demo Project](https://github.com/quyvu01/FlowXDemo)

## Features

- Fluent API for building Request Flows.
- Error handling mechanisms with custom error definitions.

## Getting Started

### Prerequisites

Ensure you have the following installed:

- .NET 8.0 or higher

### Installation

To install the FlowX package, use the following NuGet command:

```bash
dotnet add package FlowX
```

Or via the NuGet Package Manager:

```bash
Install-Package FlowX
```

## Usage

### 1. Register FlowX in the Dependency Injection Container

Add the FlowX to your service configuration to register OfX:

```csharp

builder.Services.AddFlowX(cfg =>
{
    cfg.AddModelsFromNamespaceContaining<ITestAssemblyMarker>();
    cfg.AddHandlersFromNamespaceContaining<ITestAssemblyMarker>();
    cfg.AddSqlPipelines(c => c.OfType<SomeThingPipeline>(serviceLifetime));
});

```

#### Method descriptions

`AddModelsFromNamespaceContaining<T>`: Registers all models located in the namespace containing the specified assembly
marker `(T)`. Use this to automatically include models you’ve created in the same assembly..
`AddHandlersFromNamespaceContaining<T>` Registers all handlers from the namespace containing the specified assembly
marker `(T)`. This ensures any handlers you’ve implemented in that assembly are properly added.
`AddSqlPipelines` Adds SQL pipelines for processing specific or generic requests. Customize the pipeline behavior using
the provided configuration.!

Below is a sample implementation of a `CreateSomeThingHandler` using FlowX(FlowX.EntityFramework sample):

```csharp

public sealed class CreateSomeThingHandler(
    ISqlRepository<SomeThing> sqlRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork)
    : EfCommandOneVoidHandler<SomeThing, CreateSomeThingCommand>(sqlRepository, unitOfWork)
{
    protected override ICommandOneFlowBuilderVoid<SomeThing> BuildCommand(
        IStartOneCommandVoid<SomeThing> fromFlow, CreateSomeThingCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateOne(mapper.Map<SomeThing>(command))
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(SomeErrorDetail());
}
```

Sample pipeline:

```csharp
public sealed class SomeThingPipeline : ISqlPipelineBehavior<GetSomeThingQuery, OneOf<SomeThingResponse, Error>>
{
    public async Task<OneOf<SomeThingResponse, Error>> HandleAsync(RequestContext<GetSomeThingQuery> requestContext,
        Func<Task<OneOf<SomeThingResponse, Error>>> next)
    {
        Console.WriteLine("GetUserPipeline");
        var result = await next.Invoke();
        return result;
    }
}
```

How to invoke the request:

```csharp
var sender = ServiceProvider.GetRequiredService<IFlowXSender>();
var userResult = await sender.ExecuteAsync(new GetUserQuery("1"));
```

Like the Mediator Pattern, you don't need to care about the handler and how it do. Just ExecuteAsync

## Key Concepts

### Request Flows

FlowX enables the creation of requests in a structured and intuitive way. Each flow defines a step-by-step process that
ensures reliability and maintainability.

#### Error Handling

Errors in FlowX are predefined, enabling consistent error messaging across your application.

## Contributing

We welcome contributions to FlowX! To contribute, please:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Submit a pull request with a detailed explanation of your changes.

## License

FlowX is licensed under the MIT License. See `LICENSE` for more details.

## Contact

For inquiries, reach out to [FlowX](https://github.com/quyvu01/FlowX).

---

Happy coding with FlowX!

| Package Name                                                                           | Description                                                                                         | .NET Version | Document                                                                                     |
|----------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------|--------------|----------------------------------------------------------------------------------------------|
| [FlowX](https://www.nuget.org/packages/FlowX/)                                         | FlowX core                                                                                          | 8.0, 9.0     | This Document                                                                                |
| **Data Provider**                                                                      |                                                                                                     |
| [FlowX.EntityFrameworkCore](https://www.nuget.org/packages/FlowX.EntityFrameworkCore/) | This is the FlowX extension package using EntityFramework to fetch data                             | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.EntityFrameworkCore/README.md) |
| **Transports**                                                                         |                                                                                                     |
| [FlowX.Nats](https://www.nuget.org/packages/FlowX.Nats/)                               | FlowX.Nats is an extension package for FlowX that leverages Nats for efficient data transportation. | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.Nats/README.md)                |
