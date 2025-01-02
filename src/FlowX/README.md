# FlowX

FlowX is a modern library designed to simplify the creation and management of Request Flow patterns in .NET applications. This library is ideal for developers building enterprise-grade solutions who need a seamless approach to implementing CQRS (Command Query Responsibility Segregation) patterns.

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
});

```


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

## Key Concepts

### Request Flows
FlowX enables the creation of requests in a structured and intuitive way. Each flow defines a step-by-step process that ensures reliability and maintainability.

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


| Package Name                                                              | Description                                                             | .NET Version | Document                                                                                     |
|---------------------------------------------------------------------------|-------------------------------------------------------------------------|--------------|----------------------------------------------------------------------------------------------|
| [FlowX](https://www.nuget.org/packages/FlowX/)                            | FlowX core                                                              | 8.0, 9.0     | This Document                                                                                |
| [FlowX-EFCore](https://www.nuget.org/packages/FlowX.EntityFrameworkCore/) | This is the FlowX extension package using EntityFramework to fetch data | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.EntityFrameworkCore/README.md) |


