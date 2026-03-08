# FlowX Pipeline Architecture

## Namespaces

- Command Pipeline: `FlowX.Abstractions.RequestFlow.Commands.PipelineFlow`
- Query Pipeline: `FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow`

---

## Command Pipeline

### Entry Points (`IStartCommandPipeline`)

```
CreateOne<T>(model | func | asyncFunc)     → IPipelineOneCreateStep<T>
CreateMany<T>(models | func | asyncFunc)   → IPipelineManyCreateStep<T>
UpdateOne<T>(filter)                       → IPipelineOneUpdateStep<T>
UpdateMany<T>(filter)                      → IPipelineManyUpdateStep<T>
RemoveOne<T>(filter)                       → IPipelineOneRemoveStep<T>
RemoveMany<T>(filter)                      → IPipelineManyRemoveStep<T>
```

### Step Interfaces

**One steps** — operate on single entity, output `TModel`:

| Interface | Methods |
|-----------|---------|
| `IPipelineOneCreateStep<T>` | `WithCondition(Func<T, OneOf<None, Error>>)`, `WithModify(Action<T>)` |
| `IPipelineOneUpdateStep<T>` | `WithErrorIfNull(Error)`, `WithCondition(...)`, `WithModify(...)` |
| `IPipelineOneRemoveStep<T>` | `WithErrorIfNull(Error)`, `WithCondition(...)` |

All One steps inherit `IPipelineNextable<T>`.

**Many steps** — operate on collection, output `List<TModel>`:

| Interface | Methods |
|-----------|---------|
| `IPipelineManyCreateStep<T>` | `WithCondition(Func<List<T>, OneOf<None, Error>>)`, `WithModify(Action<T>)` |
| `IPipelineManyUpdateStep<T>` | `WithCondition(...)`, `WithModify(Action<List<T>>)` |
| `IPipelineManyRemoveStep<T>` | `WithCondition(...)` |

All Many steps inherit `IPipelineNextable<List<T>>`.

### Interface Hierarchy

```
IPipelineContinuation<out TPrev>           ← transitions only (Then*)
  ↑
IPipelineNextable<out TPrev>               ← transitions + Done + terminals
  ↑
IPipelineOneCreateStep<T> / IPipelineOneUpdateStep<T> / ...

IPipelineContinuationOrTerminal<out TPrev> ← IPipelineContinuation + IPipelineFlowBuilder
  (returned by Done().WithErrorIfSaveChange)
```

### Chaining (`IPipelineContinuation<TPrev>`)

After configuring a step (WithCondition, WithModify, etc.), transitions are available:

```
.ThenCreateOne<T>(Func<TPrev, T>)
.ThenCreateMany<T>(Func<TPrev, IEnumerable<T>>)
.ThenUpdateOne<T>(Func<TPrev, Expression<Func<T, bool>>>)
.ThenUpdateMany<T>(Func<TPrev, Expression<Func<T, bool>>>)
.ThenRemoveOne<T>(Func<TPrev, Expression<Func<T, bool>>>)
.ThenRemoveMany<T>(Func<TPrev, Expression<Func<T, bool>>>)
```

### Transaction Boundary (`.Done()`)

`.Done()` marks a save point (transaction boundary). **After `.Done()`, `.WithErrorIfSaveChange(error)` is REQUIRED before continuing.**

```
.Done()                              → IPipelineAfterDone<TPrev>
  .WithErrorIfSaveChange(error)      → IPipelineContinuationOrTerminal<TPrev>
    .ThenCreateOne(...)              → new step chain
    .ThenCreateMany(...)
    ...
  .WithResultIfSucceed(func)         → IPipelineResultTerminal<R>
```

`IPipelineContinuationOrTerminal<TPrev>` inherits both `IPipelineContinuation<TPrev>` and `IPipelineFlowBuilder`, so it can either continue chaining or be used as the final builder (void handler).

### Terminals

Without Done:

```
.WithErrorIfSaveChange(error)    → IPipelineTerminal (void handler)
.WithResultIfSucceed<R>(func)    → IPipelineResultTerminal<R> (result handler)
```

With hooks (on terminal interfaces):

```
.WithBeforeExecution(action)     → IPipelineTerminal / IPipelineResultTerminal
.WithAfterExecution(action)      → IPipelineFlowBuilder / IPipelineResultFlowBuilder
```

### Complete Flow Examples

**Simple create:**
```csharp
flow.CreateOne(new Order { ... })
    .WithErrorIfSaveChange(new Error("Failed"));
```

**Create with validation + result:**
```csharp
flow.CreateOne(new Order { ... })
    .WithCondition(o => o.CustomerName != null ? None.Value : new Error("Name required"))
    .WithModify(o => o.CreatedAt = DateTime.UtcNow)
    .WithResultIfSucceed<Guid>(o => o.Id);
```

**Multi-step with Done (separate transactions):**
```csharp
flow.CreateOne(new Order { ... })
    .Done()
    .WithErrorIfSaveChange(new Error("Order save failed"))
    .ThenCreateMany<OrderItem>(order => items.Select(i => new OrderItem { OrderId = order.Id, ... }))
    .WithResultIfSucceed<int>(items => items.Count)
    .WithErrorIfSaveChange(new Error("Items save failed"));
```

**Batch operations:**
```csharp
flow.CreateMany(products)
    .WithModify(p => p.CreatedAt = DateTime.UtcNow)
    .ThenUpdateOne<Inventory>(products => inv => inv.ProductId == products.First().Id)
    .WithModify(inv => inv.Stock += products.Count)
    .WithErrorIfSaveChange(new Error("Failed"));
```

### Service Provider (`IPipelineServiceProvider`)

```csharp
Task<TModel> CreateOneAsync<TModel>(TModel model, CancellationToken ct);
Task<List<TModel>> CreateManyAsync<TModel>(List<TModel> models, CancellationToken ct);
Task<TModel> GetFirstByConditionAsync<TModel>(Expression<Func<TModel, bool>> filter, CancellationToken ct);
Task<List<TModel>> GetManyByConditionAsync<TModel>(Expression<Func<TModel, bool>> filter, CancellationToken ct);
Task RemoveOneAsync<TModel>(TModel model, CancellationToken ct);
Task RemoveManyAsync<TModel>(List<TModel> models, CancellationToken ct);
Task SaveChangesAsync(CancellationToken ct);
```

---

## Query Pipeline

### Entry Points (`IStartQueryPipeline`)

```
QueryOne<T>(filter)                              → IQueryPipelineOneStep<T>
QueryMany<T>(filter)                             → IQueryPipelineManyStep<T>
QueryPaginated<T>(filter, skip, take, sortFields) → IQueryPipelinePaginatedStep<T>
QueryCounting<T>(filter)                         → IQueryPipelineCountingStep<T>
```

### Step Interfaces

| Interface | Output Type | Methods |
|-----------|-------------|---------|
| `IQueryPipelineOneStep<T>` | `T` | `WithSpecialAction(...)`, `WithErrorIfNull(Error)` |
| `IQueryPipelineManyStep<T>` | `List<T>` | `WithSpecialAction(...)`, `WithDefaultSortFields(ExpressionOrder<T>)` |
| `IQueryPipelinePaginatedStep<T>` | `QueryPipelinePage<T>` | `WithSpecialAction(...)`, `WithDefaultSortFields(...)` |
| `IQueryPipelineCountingStep<T>` | `long` | `WithSpecialAction(...)` |

All inherit `IQueryPipelineNextable<TOutput>`.

### Chaining (`IQueryPipelineNextable<TPrev>`)

Two flavors per transition:

```
// Filter-based
.ThenQueryOne<T>(Func<TPrev, Expression<Func<T, bool>>>)
.ThenQueryMany<T>(Func<TPrev, Expression<Func<T, bool>>>)
.ThenQueryPaginated<T>(Func<TPrev, Expression<Func<T, bool>>>, skip, take, sortFields)
.ThenQueryCounting<T>(Func<TPrev, Expression<Func<T, bool>>>)

// Queryable-based (full IQueryable control)
.ThenQueryOneFromQueryable<T>(Func<TPrev, IQueryable<T>, IQueryable<T>>)
.ThenQueryManyFromQueryable<T>(...)
.ThenQueryPaginatedFromQueryable<T>(...)
.ThenQueryCountingFromQueryable<T>(...)
```

### Terminal

```
.WithResult<R>(Func<TPrev, R>)           → IQueryPipelineResultTerminal<R>
.WithBeforeExecution(action)              → hooks
.WithAfterExecution(action)               → hooks
```

### Service Provider (`IQueryPipelineServiceProvider`)

```csharp
Task<TModel> GetFirstByConditionAsync<TModel>(filter, specialAction, ct);
Task<List<TModel>> GetManyByConditionAsync<TModel>(filter, specialAction, ct);
Task<QueryPipelinePage<TModel>> GetManyWithPaginationAsync<TModel>(filter, specialAction, defaultSort, sortedFields, skip, take, ct);
Task<long> GetCountAsync<TModel>(filter, specialAction, ct);
```

---

## Architecture Notes

- **Step descriptors** (internal): `CreateOnePipelineStep<TModel, TPrev>`, `CreateManyPipelineStep<TModel, TPrev>`, etc. — store config and execute via provider
- **Configurators** (internal): `PipelineOneStepConfigurator<TModel, TPrev>`, `PipelineManyStepConfigurator<TModel, TPrev>` — implement step interfaces via explicit interface implementations
- **EF Core providers**: `EfPipelineServiceProvider` (commands), `EfQueryPipelineServiceProvider` (queries) — implement provider interfaces using `IUnitOfWork` / `IRepository<T>`
- **`ExpressionOrder<T>`**: Fluent sort builder — `ExpressionOrder<T>.Of(x => x.Field)`, `.ThenDescBy(x => x.Other)`
