using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;
using FlowX.Errors;
using FlowX.Structs;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class CommandPipelineFlowTests
{
    // === Test Models ===

    private sealed class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
    }

    private sealed class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
    }

    private sealed class Inventory
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    private sealed class AuditLog
    {
        public string Message { get; set; }
    }

    // === In-Memory Service Provider ===

    private sealed class InMemoryServiceProvider : IPipelineServiceProvider
    {
        private readonly Dictionary<Type, List<object>> _store = new();
        public List<string> Operations { get; } = [];

        public void Seed<T>(T item) where T : class
        {
            if (!_store.ContainsKey(typeof(T))) _store[typeof(T)] = [];
            _store[typeof(T)].Add(item);
        }

        public Task<TModel> CreateOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class
        {
            if (!_store.ContainsKey(typeof(TModel))) _store[typeof(TModel)] = [];
            _store[typeof(TModel)].Add(model);
            Operations.Add($"Create:{typeof(TModel).Name}");
            return Task.FromResult(model);
        }

        public Task<TModel> GetFirstByConditionAsync<TModel>(
            Expression<Func<TModel, bool>> filter, CancellationToken ct) where TModel : class
        {
            Operations.Add($"Get:{typeof(TModel).Name}");
            if (!_store.TryGetValue(typeof(TModel), out var list)) return Task.FromResult<TModel>(null);
            var compiled = filter.Compile();
            return Task.FromResult(list.Cast<TModel>().FirstOrDefault(compiled));
        }

        public Task RemoveOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class
        {
            Operations.Add($"Remove:{typeof(TModel).Name}");
            _store.GetValueOrDefault(typeof(TModel))?.Remove(model);
            return Task.CompletedTask;
        }

        public Task<List<TModel>> CreateManyAsync<TModel>(List<TModel> models, CancellationToken ct)
            where TModel : class
        {
            Operations.Add($"CreateMany:{typeof(TModel).Name}");
            var news = models.ToList();
            news.ForEach(model => _store[typeof(TModel)].Add(model));
            return Task.FromResult(news);
        }

        public Task<List<TModel>> GetManyByConditionAsync<TModel>(Expression<Func<TModel, bool>> filter,
            CancellationToken ct) where TModel : class
        {
            Operations.Add($"GetMany:{typeof(TModel).Name}");
            if (!_store.TryGetValue(typeof(TModel), out var list)) return Task.FromResult<List<TModel>>([]);
            var compiled = filter.Compile();
            return Task.FromResult<List<TModel>>([..list.OfType<TModel>().Where(compiled)]);
        }

        public Task RemoveManyAsync<TModel>(List<TModel> models, CancellationToken ct) where TModel : class
        {
            Operations.Add($"Remove:{typeof(TModel).Name}");
            var stored = _store.GetValueOrDefault(typeof(TModel));
            models.ForEach(model => stored?.Remove(model));
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            Operations.Add("SaveChanges");
            return Task.CompletedTask;
        }
    }

    // === Helper: execute pipeline steps ===

    private static async Task<object> ExecutePipeline(IPipelineFlowBuilder builder, IPipelineServiceProvider provider)
    {
        object prev = null;
        foreach (var step in builder.Steps)
        {
            prev = await step.ExecuteAsync(provider, prev, CancellationToken.None);
            if (step.IsTransactionBoundary)
                await provider.SaveChangesAsync(CancellationToken.None);
        }

        await provider.SaveChangesAsync(CancellationToken.None);
        return prev;
    }

    private static async Task<TResult> ExecuteResultPipeline<TResult>(
        IPipelineResultFlowBuilder<TResult> builder, IPipelineServiceProvider provider)
    {
        if (builder.BeforeExecutionFunc is { } beforeFunc)
            await beforeFunc.Invoke();

        object prev = null;
        foreach (var step in builder.Steps)
        {
            prev = await step.ExecuteAsync(provider, prev, CancellationToken.None);
            if (step.IsTransactionBoundary)
                await provider.SaveChangesAsync(CancellationToken.None);
        }

        try
        {
            await provider.SaveChangesAsync(CancellationToken.None);
        }
        catch when (builder.SaveChangesError is { } saveChangesError)
        {
            throw saveChangesError;
        }

        if (builder.AfterExecutionFunc is { } afterFunc)
            await afterFunc.Invoke();

        return await builder.ResultFuncAsync(prev);
    }

    #region Single Step

    [Fact]
    public async Task SingleCreate_ShouldCreateAndSave()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithErrorIfSaveChange(new Error("Failed"));

        var result = await ExecutePipeline(builder, provider);

        Assert.IsType<Order>(result);
        Assert.Equal("Alice", ((Order)result).CustomerName);
        Assert.Equal(["Create:Order", "SaveChanges"], provider.Operations);
    }

    [Fact]
    public async Task SingleCreate_WithFactory_ShouldCreate()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(() => new Order { Id = Guid.NewGuid(), CustomerName = "Bob" })
            .WithErrorIfSaveChange(new Error("Failed"));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Bob", ((Order)result).CustomerName);
    }

    [Fact]
    public async Task SingleCreate_WithAsyncFactory_ShouldCreate()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(async () =>
            {
                await Task.Delay(1);
                return new Order { Id = Guid.NewGuid(), CustomerName = "Charlie" };
            })
            .WithErrorIfSaveChange(new Error("Failed"));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Charlie", ((Order)result).CustomerName);
    }

    #endregion

    #region Multi-Step Chaining

    [Fact]
    public async Task TwoSteps_Create_ThenCreate_PreviousResultPassed()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Alice" })
            .ThenCreateOne<OrderItem>(order => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Product = "Widget",
                Quantity = 3
            })
            .WithErrorIfSaveChange(new Error("Failed"));

        var result = await ExecutePipeline(builder, provider);

        var item = Assert.IsType<OrderItem>(result);
        Assert.Equal(orderId, item.OrderId);
        Assert.Equal("Widget", item.Product);
        Assert.Equal(["Create:Order", "Create:OrderItem", "SaveChanges"], provider.Operations);
    }

    [Fact]
    public async Task ThreeSteps_Create_ThenCreate_ThenUpdate()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });

        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .ThenCreateOne<OrderItem>(order => new OrderItem
            {
                OrderId = order.Id,
                Product = "Widget",
                Quantity = 2
            })
            .ThenUpdateOne<Inventory>(item =>
                inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Inventory not found"))
            .WithModify(inv => inv.Quantity -= 2)
            .WithErrorIfSaveChange(new Error("Failed"));

        await ExecutePipeline(builder, provider);

        Assert.Equal(
            ["Create:Order", "Create:OrderItem", "Get:Inventory", "SaveChanges"],
            provider.Operations);
    }

    #endregion

    #region .Done() Transaction Boundaries

    [Fact]
    public async Task Done_ShouldInsertSaveChanges()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Alice" })
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenCreateOne<OrderItem>(order => new OrderItem { OrderId = order.Id, Product = "A" })
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenCreateOne<AuditLog>(_ => new AuditLog { Message = "done" })
            .WithErrorIfSaveChange(new Error("Failed"));

        await ExecutePipeline(builder, provider);

        Assert.Equal(
            [
                "Create:Order", "SaveChanges", // Done #1
                "Create:OrderItem", "SaveChanges", // Done #2
                "Create:AuditLog", "SaveChanges" // Final
            ],
            provider.Operations);
    }

    [Fact]
    public async Task NoDone_SingleSaveChanges()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .ThenCreateOne<OrderItem>(order => new OrderItem { OrderId = order.Id, Product = "A" })
            .ThenCreateOne<AuditLog>(_ => new AuditLog { Message = "done" })
            .WithErrorIfSaveChange(new Error("Failed"));

        await ExecutePipeline(builder, provider);

        Assert.Equal(
            ["Create:Order", "Create:OrderItem", "Create:AuditLog", "SaveChanges"],
            provider.Operations);
    }

    [Fact]
    public void Done_SetsTransactionBoundary()
    {
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid() })
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenCreateOne<OrderItem>(_ => new OrderItem())
            .WithErrorIfSaveChange(new Error("Failed"));

        Assert.True(builder.Steps[0].IsTransactionBoundary);
        Assert.False(builder.Steps[1].IsTransactionBoundary);
    }

    #endregion

    #region Conditions

    [Fact]
    public async Task Condition_PassingCondition_ShouldContinue()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error("Failed"));

        var result = await ExecutePipeline(builder, provider);
        Assert.IsType<Order>(result);
    }

    [Fact]
    public async Task Condition_FailingCondition_ShouldThrow()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithCondition(_ => new Error("Validation failed"))
            .WithErrorIfSaveChange(new Error("Failed"));

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("Validation failed", ex.Message);
        Assert.DoesNotContain("SaveChanges", provider.Operations);
    }

    [Fact]
    public async Task ChainedConditions_FailFast()
    {
        var secondConditionCalled = false;
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid() })
            .WithCondition(_ => new Error("First fails"))
            .WithCondition(_ =>
            {
                secondConditionCalled = true;
                return None.Value;
            })
            .WithErrorIfSaveChange(new Error("Failed"));

        await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.False(secondConditionCalled);
    }

    [Fact]
    public async Task ChainedConditions_AllPass()
    {
        var callOrder = new List<int>();
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid() })
            .WithCondition(_ =>
            {
                callOrder.Add(1);
                return None.Value;
            })
            .WithCondition(_ =>
            {
                callOrder.Add(2);
                return None.Value;
            })
            .WithCondition(_ =>
            {
                callOrder.Add(3);
                return None.Value;
            })
            .WithErrorIfSaveChange(new Error("Failed"));

        await ExecutePipeline(builder, provider);
        Assert.Equal([1, 2, 3], callOrder);
    }

    #endregion

    #region WithModify

    [Fact]
    public async Task WithModify_ShouldMutateBeforeCreate()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Draft" })
            .WithModify(o => o.CustomerName = "Final")
            .WithErrorIfSaveChange(new Error("Failed"));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Final", ((Order)result).CustomerName);
    }

    [Fact]
    public async Task UpdateStep_WithModify_ShouldMutateEntity()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });

        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .UpdateOne<Inventory>(inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithModify(inv => inv.Quantity = 5)
            .WithErrorIfSaveChange(new Error("Failed"));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(5, ((Inventory)result).Quantity);
    }

    #endregion

    #region Update/Remove ErrorIfNull

    [Fact]
    public async Task UpdateStep_NotFound_ShouldThrowNullError()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .UpdateOne<Inventory>(inv => inv.ProductId == Guid.NewGuid())
            .WithErrorIfNull(new Error("Inventory not found"))
            .WithErrorIfSaveChange(new Error("Failed"));

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("Inventory not found", ex.Message);
    }

    [Fact]
    public async Task RemoveStep_NotFound_ShouldThrowNullError()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .RemoveOne<Inventory>(inv => inv.ProductId == Guid.NewGuid())
            .WithErrorIfNull(new Error("Not found"))
            .WithErrorIfSaveChange(new Error("Failed"));

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("Not found", ex.Message);
    }

    [Fact]
    public async Task RemoveStep_Found_ShouldRemove()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });

        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .RemoveOne<Inventory>(inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithErrorIfSaveChange(new Error("Failed"));

        await ExecutePipeline(builder, provider);
        Assert.Contains("Remove:Inventory", provider.Operations);
    }

    #endregion

    #region Pipeline-Level Hooks

    [Fact]
    public void Hooks_ShouldBeSet()
    {
        var flow = new CommandPipelineFlow();
        var builder = (IPipelineFlowBuilder)((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid() })
            .WithErrorIfSaveChange(new Error("Failed"))
            .WithBeforeExecution(() => { })
            .WithAfterExecution(() => { });

        Assert.NotNull(builder.BeforeExecutionFunc);
        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task Hooks_ShouldInvoke()
    {
        var callOrder = new List<string>();
        var flow = new CommandPipelineFlow();
        var builder = (IPipelineFlowBuilder)((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid() })
            .WithErrorIfSaveChange(new Error("Failed"))
            .WithBeforeExecution(() => callOrder.Add("before"))
            .WithAfterExecution(() => callOrder.Add("after"));

        await builder.BeforeExecutionFunc();
        await builder.AfterExecutionFunc();
        Assert.Equal(["before", "after"], callOrder);
    }

    [Fact]
    public void NoHooks_ShouldBeNull()
    {
        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid() })
            .WithErrorIfSaveChange(new Error("Failed"));

        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    #endregion

    #region Closure for Non-Immediate Previous

    [Fact]
    public async Task Closure_CanAccessEarlierStepResult()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();
        Order capturedOrder = null;

        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Alice" })
            .WithCondition(o =>
            {
                capturedOrder = o;
                return None.Value;
            })
            .ThenCreateOne<OrderItem>(order => new OrderItem { OrderId = order.Id, Product = "Widget" })
            .ThenCreateOne<AuditLog>(_ => new AuditLog
            {
                Message = $"Order {capturedOrder!.Id} by {capturedOrder.CustomerName}"
            })
            .WithErrorIfSaveChange(new Error("Failed"));

        await ExecutePipeline(builder, provider);

        Assert.NotNull(capturedOrder);
        Assert.Equal(orderId, capturedOrder.Id);
    }

    #endregion

    #region Mixed Operations (Create → Update → Remove)

    [Fact]
    public async Task MixedSteps_Create_Update_Remove()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });
        provider.Seed(new AuditLog { Message = "old log" });

        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .ThenUpdateOne<Inventory>(_ => inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithModify(inv => inv.Quantity -= 1)
            .ThenRemoveOne<AuditLog>(_ => log => log.Message == "old log")
            .WithErrorIfNull(new Error("Log not found"))
            .WithErrorIfSaveChange(new Error("Failed"));

        await ExecutePipeline(builder, provider);

        Assert.Equal(
            ["Create:Order", "Get:Inventory", "Get:AuditLog", "Remove:AuditLog", "SaveChanges"],
            provider.Operations);
    }

    #endregion

    #region Full Pipeline with Done + Hooks

    [Fact]
    public async Task FullPipeline_WithDone_Hooks_Conditions()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });

        var hookOrder = new List<string>();
        var flow = new CommandPipelineFlow();

        var builder = (IPipelineFlowBuilder)((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithCondition(_ => None.Value)
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenCreateOne<OrderItem>(order => new OrderItem { OrderId = order.Id, Product = "Widget", Quantity = 2 })
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenUpdateOne<Inventory>(_ => inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithModify(inv => inv.Quantity -= 2)
            .WithCondition(inv => inv.Quantity >= 0 ? None.Value : new Error("Out of stock"))
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenCreateOne<AuditLog>(_ => new AuditLog { Message = "Completed" })
            .WithErrorIfSaveChange(new Error("Failed"))
            .WithBeforeExecution(() => hookOrder.Add("before"))
            .WithAfterExecution(() => hookOrder.Add("after"));

        // Execute with hooks
        await builder.BeforeExecutionFunc!();
        await ExecutePipeline(builder, provider);
        await builder.AfterExecutionFunc!();

        Assert.Equal(["before", "after"], hookOrder);
        Assert.Equal(
            [
                "Create:Order", "SaveChanges",
                "Create:OrderItem", "SaveChanges",
                "Get:Inventory", "SaveChanges",
                "Create:AuditLog", "SaveChanges"
            ],
            provider.Operations);
    }

    #endregion

    #region Interface Reflection

    [Fact]
    public void IStartPipeline_ShouldExist()
    {
        Assert.True(typeof(IStartCommandPipeline).IsInterface);
    }

    [Fact]
    public void IPipelineCreateStep_ShouldInherit_IPipelineNextable()
    {
        var interfaces = typeof(IPipelineOneCreateStep<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineNextable<>));
    }

    [Fact]
    public void IPipelineUpdateStep_ShouldInherit_IPipelineNextable()
    {
        var interfaces = typeof(IPipelineOneUpdateStep<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineNextable<>));
    }

    [Fact]
    public void IPipelineRemoveStep_ShouldInherit_IPipelineNextable()
    {
        var interfaces = typeof(IPipelineOneRemoveStep<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineNextable<>));
    }

    [Fact]
    public void IPipelineTerminal_ShouldInherit_IPipelineFlowBuilder()
    {
        var interfaces = typeof(IPipelineTerminal).GetInterfaces();
        Assert.Contains(interfaces, i => i == typeof(IPipelineFlowBuilder));
    }

    [Fact]
    public void StepCount_MatchesChainLength()
    {
        var flow = new CommandPipelineFlow();
        IPipelineFlowBuilder builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid() })
            .ThenCreateOne<OrderItem>(_ => new OrderItem())
            .ThenCreateOne<AuditLog>(_ => new AuditLog())
            .WithErrorIfSaveChange(new Error("Failed"));

        Assert.Equal(3, builder.Steps.Count);
    }

    #endregion

    #region Pipeline Result — Single Step

    [Fact]
    public async Task Result_SingleCreate_SyncResultFunc()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Alice" })
            .WithResultIfSucceed<Guid>(order => order.Id);

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal(orderId, result);
        Assert.Equal(["Create:Order", "SaveChanges"], provider.Operations);
    }

    [Fact]
    public async Task Result_SingleCreate_AsyncResultFunc()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Bob" })
            .WithResultIfSucceed<string>(async order =>
            {
                await Task.Delay(1);
                return order.CustomerName;
            });

        var result = await ExecuteResultPipeline(builder, provider);
        Assert.Equal("Bob", result);
    }

    [Fact]
    public async Task Result_SingleUpdate_ReturnsTransformedResult()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });

        var flow = new CommandPipelineFlow();
        var builder = ((IStartCommandPipeline)flow)
            .UpdateOne<Inventory>(inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithModify(inv => inv.Quantity -= 3)
            .WithResultIfSucceed<int>(inv => inv.Quantity);

        var result = await ExecuteResultPipeline(builder, provider);
        Assert.Equal(7, result);
    }

    #endregion

    #region Pipeline Result — Multi-Step Chain

    [Fact]
    public async Task Result_MultiStep_ResultFromLastStep()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Alice" })
            .ThenCreateOne<OrderItem>(order => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Product = "Widget",
                Quantity = 5
            })
            .WithResultIfSucceed<string>(item => $"{item.Product}:{item.Quantity}");

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal("Widget:5", result);
        Assert.Equal(["Create:Order", "Create:OrderItem", "SaveChanges"], provider.Operations);
    }

    [Fact]
    public async Task Result_ThreeSteps_ResultFromLastStep()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });

        var flow = new CommandPipelineFlow();
        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .ThenCreateOne<OrderItem>(order => new OrderItem
            {
                OrderId = order.Id,
                Product = "Widget",
                Quantity = 2
            })
            .ThenUpdateOne<Inventory>(_ => inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithModify(inv => inv.Quantity -= 2)
            .WithResultIfSucceed<int>(inv => inv.Quantity);

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal(8, result);
    }

    #endregion

    #region Pipeline Result — After Done

    [Fact]
    public async Task Result_AfterDone_WithResultIfSucceed()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Alice" })
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenCreateOne<OrderItem>(order => new OrderItem
            {
                OrderId = order.Id,
                Product = "Widget",
                Quantity = 3
            })
            .WithResultIfSucceed<Guid>(item => item.OrderId);

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal(orderId, result);
        Assert.Equal(
            ["Create:Order", "SaveChanges", "Create:OrderItem", "SaveChanges"],
            provider.Operations);
    }

    [Fact]
    public async Task Result_AfterDone_AsyncResultFunc()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Charlie" })
            .Done().WithErrorIfSaveChange(new Error("Failed"))
            .ThenCreateOne<AuditLog>(order => new AuditLog { Message = order.CustomerName })
            .WithResultIfSucceed<string>(async log =>
            {
                await Task.Delay(1);
                return $"Logged: {log.Message}";
            });

        var result = await ExecuteResultPipeline(builder, provider);
        Assert.Equal("Logged: Charlie", result);
    }

    #endregion

    #region Pipeline Result — Hooks and Error Handling

    [Fact]
    public async Task Result_WithHooks_ExecutesInOrder()
    {
        var callOrder = new List<string>();
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithResultIfSucceed<string>(order => order.CustomerName)
            .WithBeforeExecution(() => callOrder.Add("before"))
            .WithAfterExecution(() => callOrder.Add("after"));

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal("Alice", result);
        Assert.Equal(["before", "after"], callOrder);
    }

    [Fact]
    public async Task Result_WithAsyncHooks_ExecutesInOrder()
    {
        var callOrder = new List<string>();
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Bob" })
            .WithResultIfSucceed<string>(order => order.CustomerName)
            .WithBeforeExecution(async () =>
            {
                await Task.Delay(1);
                callOrder.Add("before-async");
            })
            .WithAfterExecution(async () =>
            {
                await Task.Delay(1);
                callOrder.Add("after-async");
            });

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal("Bob", result);
        Assert.Equal(["before-async", "after-async"], callOrder);
    }

    [Fact]
    public async Task Result_WithErrorIfSaveChange_ShouldThrowOnSaveFailure()
    {
        var provider = new FailingSaveProvider();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithResultIfSucceed<string>(order => order.CustomerName)
            .WithErrorIfSaveChange(new Error("Save failed!"));

        var ex = await Assert.ThrowsAsync<Error>(() => ExecuteResultPipeline(builder, provider));
        Assert.Equal("Save failed!", ex.Message);
    }

    [Fact]
    public async Task Result_ConditionFail_ShouldThrowBeforeResult()
    {
        var resultFuncCalled = false;
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithCondition(_ => new Error("Validation failed"))
            .WithResultIfSucceed<string>(order =>
            {
                resultFuncCalled = true;
                return order.CustomerName;
            });

        var ex = await Assert.ThrowsAsync<Error>(() => ExecuteResultPipeline(builder, provider));
        Assert.Equal("Validation failed", ex.Message);
        Assert.False(resultFuncCalled);
    }

    [Fact]
    public async Task Result_ConditionFail_InLaterStep_ShouldThrow()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 0 });

        var flow = new CommandPipelineFlow();
        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .ThenUpdateOne<Inventory>(_ => inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithCondition(inv => inv.Quantity > 0 ? None.Value : new Error("Out of stock"))
            .WithResultIfSucceed<int>(inv => inv.Quantity);

        var ex = await Assert.ThrowsAsync<Error>(() => ExecuteResultPipeline(builder, provider));
        Assert.Equal("Out of stock", ex.Message);
    }

    [Fact]
    public async Task Result_NullError_InUpdateStep_ShouldThrow()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .UpdateOne<Inventory>(inv => inv.ProductId == Guid.NewGuid())
            .WithErrorIfNull(new Error("Inventory not found"))
            .WithResultIfSucceed<int>(inv => inv.Quantity);

        var ex = await Assert.ThrowsAsync<Error>(() => ExecuteResultPipeline(builder, provider));
        Assert.Equal("Inventory not found", ex.Message);
    }

    #endregion

    #region Pipeline Result — Complex Scenarios

    [Fact]
    public async Task Result_FullPipeline_WithDone_Hooks_Conditions_Result()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryServiceProvider();
        provider.Seed(new Inventory { ProductId = productId, Quantity = 10 });

        var hookOrder = new List<string>();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Alice" })
            .WithCondition(_ => None.Value)
            .Done()
            .WithErrorIfSaveChange(new Error("Save failed!"))
            .ThenCreateOne<OrderItem>(order => new OrderItem
            {
                OrderId = order.Id,
                Product = "Widget",
                Quantity = 2
            })
            .Done()
            .WithErrorIfSaveChange(new Error("Save failed!"))
            .ThenUpdateOne<Inventory>(_ => inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithModify(inv => inv.Quantity -= 2)
            .WithCondition(inv => inv.Quantity >= 0 ? None.Value : new Error("Out of stock"))
            .WithResultIfSucceed<int>(inv => inv.Quantity)
            .WithBeforeExecution(() => hookOrder.Add("before"))
            .WithAfterExecution(() => hookOrder.Add("after"));

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal(8, result);
        Assert.Equal(["before", "after"], hookOrder);
        Assert.Equal(
            [
                "Create:Order", "SaveChanges",
                "Create:OrderItem", "SaveChanges",
                "Get:Inventory", "SaveChanges"
            ],
            provider.Operations);
    }

    [Fact]
    public async Task Result_ComplexObject_AsResult()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();
        var orderId = Guid.NewGuid();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = orderId, CustomerName = "Alice" })
            .ThenCreateOne<OrderItem>(order => new OrderItem
            {
                OrderId = order.Id,
                Product = "Widget",
                Quantity = 3
            })
            .WithResultIfSucceed<OrderItem>(item => item);

        var result = await ExecuteResultPipeline(builder, provider);

        Assert.Equal(orderId, result.OrderId);
        Assert.Equal("Widget", result.Product);
        Assert.Equal(3, result.Quantity);
    }

    [Fact]
    public async Task Result_WithModify_ThenResult()
    {
        var provider = new InMemoryServiceProvider();
        var flow = new CommandPipelineFlow();

        var builder = ((IStartCommandPipeline)flow)
            .CreateOne(new Order { Id = Guid.NewGuid(), CustomerName = "Draft" })
            .WithModify(o => o.CustomerName = "Final")
            .WithResultIfSucceed<string>(o => o.CustomerName);

        var result = await ExecuteResultPipeline(builder, provider);
        Assert.Equal("Final", result);
    }

    #endregion

    #region Pipeline Result — Interface Reflection

    [Fact]
    public void IPipelineResultFlowBuilder_ShouldInherit_IPipelineFlowBuilder()
    {
        var interfaces = typeof(IPipelineResultFlowBuilder<>).GetInterfaces();
        Assert.Contains(interfaces, i => i == typeof(IPipelineFlowBuilder));
    }

    [Fact]
    public void IPipelineResultTerminal_ShouldInherit_IPipelineResultFlowBuilder()
    {
        var interfaces = typeof(IPipelineResultTerminal<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineResultFlowBuilder<>));
    }

    [Fact]
    public void IPipelineNextable_ShouldHave_WithResultIfSucceed()
    {
        var methods = typeof(IPipelineNextable<>).GetMethods();
        var resultMethods = methods.Where(m => m.Name == "WithResultIfSucceed").ToArray();
        Assert.Equal(2, resultMethods.Length); // sync + async overloads
    }

    [Fact]
    public void IPipelineAfterDone_ShouldHave_WithResultIfSucceed()
    {
        var methods = typeof(IPipelineAfterDone<>).GetMethods();
        var resultMethods = methods.Where(m => m.Name == "WithResultIfSucceed").ToArray();
        Assert.Equal(2, resultMethods.Length); // sync + async overloads
    }

    #endregion

    // === Failing provider for SaveChanges error tests ===

    private sealed class FailingSaveProvider : IPipelineServiceProvider
    {
        public Task<TModel> CreateOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class
            => Task.FromResult(model);

        public Task<TModel> GetFirstByConditionAsync<TModel>(
            Expression<Func<TModel, bool>> filter, CancellationToken ct) where TModel : class
            => Task.FromResult<TModel>(null);

        public Task RemoveOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class
            => Task.CompletedTask;

        public Task<List<TModel>> CreateManyAsync<TModel>(List<TModel> models, CancellationToken ct)
            where TModel : class => Task.FromResult(models);

        public Task<List<TModel>> GetManyByConditionAsync<TModel>(Expression<Func<TModel, bool>> filter,
            CancellationToken ct) where TModel : class =>
            Task.FromResult<List<TModel>>([]);

        public Task RemoveManyAsync<TModel>(List<TModel> models, CancellationToken ct) where TModel : class =>
            Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken ct)
            => throw new InvalidOperationException("DB error");
    }
}