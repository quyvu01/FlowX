# FlowX.EntityFrameworkCore

FlowX.EntityFrameworkCore is an extension package for the FlowX library, providing seamless integration with Entity
Framework Core.

## Features

- Simplified integration with EF Core repositories.
- Built-in support for the Unit of Work pattern.
- Customizable database interaction flows for greater flexibility.
- Enhanced error handling for EF Core operations, ensuring reliability.
- Intuitive fluent API for managing EF Core entities with ease.
- Dynamic repository generation based on your models. Simply implement the IEfModel interface, and the rest is handled automatically!
- Comprehensive support for CRUD operations, including methods like CreateOne, CreateMany, GetOne, GetCollection, GetPaged, and GetCount.
- Build custom request flows by inheriting classes such as EfQueryOneHandler, EfQueryCollectionHandler, and EfCommandVoidHandler...
- Effortless debugging and maintenance: With everything organized as a flow, you can easily trace issues, maintain your codebase, and scale up with confidence.

## Getting Started

### Prerequisites

Ensure you have the following installed:

- .NET 8.0 or higher
- Entity Framework Core
- FlowX

### Installation

To install the FlowX.EntityFrameworkCore package, use the following NuGet command:

```bash
dotnet add package FlowX.EntityFrameworkCore
```

Or via the NuGet Package Manager:

```bash
Install-Package FlowX.EntityFrameworkCore
```

## Usage

### Usage

Below is an example of how to use FlowX.EntityFrameworkCore to handle a `CreateSomeThingCommand`:

```csharp

builder.Services.AddFlowX(cfg =>
{
    cfg.AddModelsFromNamespaceContaining<ITestAssemblyMarker>();
    cfg.AddHandlersFromNamespaceContaining<ITestAssemblyMarker>();
    cfg.AddDbContextDynamic<TestDbContext>(options =>
    {
        options.AddDynamicRepositories();
        options.AddDynamicUnitOfWork();
    });
});
```

Examples:

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

public class UpdateSomeThingHandler(
    ISqlRepository<SomeThing> sqlRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : EfCommandOneVoidHandler<SomeThing, UpdateSomeThingCommand>(sqlRepository)
{
    protected override ICommandOneFlowBuilderVoid<SomeThing> BuildCommand(
        IStartOneCommandVoid<SomeThing> fromFlow, UpdateSomeThingCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .UpdateOne(x => x.Id == command.Id)
            .WithSpecialAction(null)
            .WithCondition(_ => None.Value)
            .WithModify(SomeThing => Mapper.Map(command, SomeThing))
            .WithErrorIfNull(MasterDataErrorDetail.SomeThingError.NotFound())
            .WithErrorIfSaveChange(SomeErrorDetail());
}

public sealed class GetSomeThingHandler(ISqlRepository<SomeThing> sqlRepository, IMapper mapper)
    : EfQueryOneHandler<SomeThing, GetSomeThingQuery, SomeThingResponse>(sqlRepository)
{
    protected override IQueryOneFlowBuilder<SomeThing, SomeThingResponse> BuildQueryFlow(
        IQueryOneFilter<SomeThing, SomeThingResponse> fromFlow, GetSomeThingQuery query)
        => fromFlow
            .WithFilter(x => x.Id == query.Id)
            .WithSpecialAction(x => x.ProjectTo<SomeThingResponse>(Mapper.ConfigurationProvider))
            .WithErrorIfNull(SomeErrorDetail());
}

// ... Other Flows

```

### Key Concepts

#### EF Core Repositories

FlowX.EntityFrameworkCore provides repository abstractions that simplify CRUD operations with EF Core. You can
inject `ISqlRepository<T>` to access database entities directly.

#### Unit of Work

Unit of Work ensures that all database operations within a command are executed in a single transaction, providing
consistency and reliability.

#### Error Handling

This extension includes advanced error handling tailored for EF Core, enabling you to catch and manage database-related
errors gracefully.

## Contributing

We welcome contributions to FlowX.EntityFrameworkCore! To contribute, please:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Submit a pull request with a detailed explanation of your changes.

## License

FlowX.EntityFrameworkCore is licensed under the MIT License. See `LICENSE` for more details.

## Contact

For inquiries, reach out to [your email or GitHub link].

---

Extend your FlowX capabilities with FlowX.EntityFrameworkCore!

| Package Name                                                                           | Description                                                             | .NET Version | Document                                                                                     |
|----------------------------------------------------------------------------------------|-------------------------------------------------------------------------|--------------|----------------------------------------------------------------------------------------------|
| [FlowX](https://www.nuget.org/packages/FlowX/)                                         | FlowX core                                                              | 8.0, 9.0     | [ReadMe](https://github.com/quyvu01/FlowX/blob/main/src/FlowX.EntityFrameworkCore/README.md) |
| [FlowX.EntityFrameworkCore](https://www.nuget.org/packages/FlowX.EntityFrameworkCore/) | This is the FlowX extension package using EntityFramework to fetch data | 8.0, 9.0     | This Document                                                                                |
