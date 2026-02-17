using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;
using FlowX.Errors;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class QueryPipelineFlowTests
{
    // === Test Models ===

    private sealed class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public bool IsActive { get; set; } = true;
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
        public int Stock { get; set; }
    }

    private sealed class ShippingInfo
    {
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; }
    }

    // === In-Memory Service Provider ===

    private sealed class InMemoryQueryServiceProvider : IQueryPipelineServiceProvider
    {
        private readonly Dictionary<Type, List<object>> _store = new();
        public List<string> Operations { get; } = [];

        public void Seed<T>(T item) where T : class
        {
            if (!_store.ContainsKey(typeof(T))) _store[typeof(T)] = [];
            _store[typeof(T)].Add(item);
        }

        public Task<TModel> GetFirstByConditionAsync<TModel>(
            Expression<Func<TModel, bool>> filter,
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
            CancellationToken ct) where TModel : class
        {
            Operations.Add($"GetOne:{typeof(TModel).Name}");
            if (!_store.TryGetValue(typeof(TModel), out var list))
                return Task.FromResult<TModel>(null);

            var queryable = list.Cast<TModel>().AsQueryable();
            if (filter is not null)
                queryable = queryable.Where(filter);
            if (specialAction is not null)
                queryable = specialAction(queryable);

            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<List<TModel>> GetManyByConditionAsync<TModel>(
            Expression<Func<TModel, bool>> filter,
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
            CancellationToken ct) where TModel : class
        {
            Operations.Add($"GetMany:{typeof(TModel).Name}");
            if (!_store.TryGetValue(typeof(TModel), out var list))
                return Task.FromResult(new List<TModel>());

            var queryable = list.Cast<TModel>().AsQueryable();
            if (filter is not null)
                queryable = queryable.Where(filter);
            if (specialAction is not null)
                queryable = specialAction(queryable);

            return Task.FromResult(queryable.ToList());
        }
    }

    // === Helper: execute query pipeline ===

    private static async Task<TResult> ExecutePipeline<TResult>(
        IQueryPipelineFlowBuilder<TResult> builder, IQueryPipelineServiceProvider provider)
    {
        if (builder.BeforeExecutionFunc is { } beforeFunc)
            await beforeFunc.Invoke();

        object prev = null;
        foreach (var step in builder.Steps)
            prev = await step.ExecuteAsync(provider, prev, CancellationToken.None);

        if (builder.AfterExecutionFunc is { } afterFunc)
            await afterFunc.Invoke();

        return await builder.ResultFuncAsync(prev);
    }

    #region Single Step — QueryOne

    [Fact]
    public async Task QueryOne_SyncResult()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithResult<string>(order => order.CustomerName);

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal("Alice", result);
        Assert.Equal(["GetOne:Order"], provider.Operations);
    }

    [Fact]
    public async Task QueryOne_AsyncResult()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Bob" });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithResult<string>(async order =>
            {
                await Task.Delay(1);
                return order.CustomerName;
            });

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Bob", result);
    }

    [Fact]
    public async Task QueryOne_WithSpecialAction()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice", IsActive = true });
        provider.Seed(new Order { Id = Guid.NewGuid(), CustomerName = "Inactive", IsActive = false });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithSpecialAction(q => q.Where(o => o.IsActive))
            .WithResult<string>(order => order.CustomerName);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Alice", result);
    }

    [Fact]
    public async Task QueryOne_WithErrorIfNull_Found()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Not found"))
            .WithResult<string>(order => order.CustomerName);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Alice", result);
    }

    [Fact]
    public async Task QueryOne_WithErrorIfNull_NotFound_Throws()
    {
        var provider = new InMemoryQueryServiceProvider();

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == Guid.NewGuid())
            .WithErrorIfNull(new Error("Order not found"))
            .WithResult<string>(order => order.CustomerName);

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("Order not found", ex.Message);
    }

    #endregion

    #region Single Step — QueryMany

    [Fact]
    public async Task QueryMany_ReturnsCollection()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget", Quantity = 3 });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Gadget", Quantity = 1 });
        provider.Seed(new OrderItem { OrderId = Guid.NewGuid(), Product = "Other", Quantity = 5 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(item => item.OrderId == orderId)
            .WithResult<int>(items => items.Count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task QueryMany_WithSpecialAction()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget", Quantity = 3 });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Gadget", Quantity = 1 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(item => item.OrderId == orderId)
            .WithSpecialAction(q => q.Where(i => i.Quantity > 2))
            .WithResult<int>(items => items.Count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(1, result);
    }

    #endregion

    #region Multi-Step — Filter-based Transitions

    [Fact]
    public async Task QueryOne_ThenQueryMany_FilterBased()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget", Quantity = 3 });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Gadget", Quantity = 1 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryMany<OrderItem>(order => item => item.OrderId == order.Id)
            .WithResult<int>(items => items.Sum(i => i.Quantity));

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal(4, result);
        Assert.Equal(["GetOne:Order", "GetMany:OrderItem"], provider.Operations);
    }

    [Fact]
    public async Task QueryOne_ThenQueryOne_FilterBased()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        provider.Seed(new ShippingInfo { OrderId = orderId, TrackingNumber = "TRACK-123" });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Order not found"))
            .ThenQueryOne<ShippingInfo>(order => s => s.OrderId == order.Id)
            .WithErrorIfNull(new Error("No shipping"))
            .WithResult<string>(s => s.TrackingNumber);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("TRACK-123", result);
    }

    [Fact]
    public async Task QueryMany_ThenQueryOne_FilterBased()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new OrderItem { Product = "Widget", Quantity = 3 });
        provider.Seed(new OrderItem { Product = "Gadget", Quantity = 1 });
        provider.Seed(new Inventory { ProductId = productId, Stock = 100 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(_ => true)
            .ThenQueryOne<Inventory>(items =>
                inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("Not found"))
            .WithResult<int>(inv => inv.Stock);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(100, result);
    }

    [Fact]
    public async Task QueryMany_ThenQueryMany()
    {
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget" });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Gadget" });
        provider.Seed(new Inventory { ProductId = productId1, Stock = 10 });
        provider.Seed(new Inventory { ProductId = productId2, Stock = 20 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(item => item.OrderId == orderId)
            .ThenQueryMany<Inventory>(_ => inv => true)
            .WithResult<int>(inventories => inventories.Sum(i => i.Stock));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(30, result);
    }

    #endregion

    #region Multi-Step — Queryable-based Transitions

    [Fact]
    public async Task ThenQueryOneFromQueryable_FullControl()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        provider.Seed(new ShippingInfo { OrderId = orderId, TrackingNumber = "TRACK-456" });
        provider.Seed(new ShippingInfo { OrderId = Guid.NewGuid(), TrackingNumber = "OTHER" });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryOneFromQueryable<ShippingInfo>((order, q) =>
                q.Where(s => s.OrderId == order.Id))
            .WithErrorIfNull(new Error("No shipping"))
            .WithResult<string>(s => s.TrackingNumber);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("TRACK-456", result);
    }

    [Fact]
    public async Task ThenQueryManyFromQueryable_FullControl()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget", Quantity = 5 });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Gadget", Quantity = 2 });
        provider.Seed(new OrderItem { OrderId = Guid.NewGuid(), Product = "Other", Quantity = 99 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryManyFromQueryable<OrderItem>((order, q) =>
                q.Where(item => item.OrderId == order.Id)
                 .Where(item => item.Quantity > 3))
            .WithResult<int>(items => items.Count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(1, result); // Only Widget (Quantity=5)
    }

    [Fact]
    public async Task QueryMany_ThenQueryOneFromQueryable()
    {
        var productId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new OrderItem { Product = "Widget", Quantity = 3 });
        provider.Seed(new Inventory { ProductId = productId, Stock = 50 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(_ => true)
            .ThenQueryOneFromQueryable<Inventory>((items, q) =>
                q.Where(inv => inv.Stock > 10))
            .WithResult<int>(inv => inv.Stock);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(50, result);
    }

    [Fact]
    public async Task QueryMany_ThenQueryManyFromQueryable()
    {
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new OrderItem { Product = "A", Quantity = 1 });
        provider.Seed(new OrderItem { Product = "B", Quantity = 2 });
        provider.Seed(new Inventory { ProductId = Guid.NewGuid(), Stock = 10 });
        provider.Seed(new Inventory { ProductId = Guid.NewGuid(), Stock = 5 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(_ => true)
            .ThenQueryManyFromQueryable<Inventory>((items, q) =>
                q.Where(inv => inv.Stock >= items.Count * 5))
            .WithResult<int>(inventories => inventories.Count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(1, result); // Only Stock=10 (>= 2*5=10)
    }

    #endregion

    #region Three-Step Chains

    [Fact]
    public async Task ThreeSteps_QueryOne_ThenMany_ThenOne()
    {
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget", Quantity = 3 });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Gadget", Quantity = 1 });
        provider.Seed(new Inventory { ProductId = productId, Stock = 42 });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryMany<OrderItem>(order => item => item.OrderId == order.Id)
            .ThenQueryOne<Inventory>(items => inv => inv.ProductId == productId)
            .WithErrorIfNull(new Error("No inventory"))
            .WithResult<int>(inv => inv.Stock);

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal(42, result);
        Assert.Equal(["GetOne:Order", "GetMany:OrderItem", "GetOne:Inventory"], provider.Operations);
    }

    [Fact]
    public async Task ThreeSteps_WithClosureCapture()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget", Quantity = 3 });
        provider.Seed(new ShippingInfo { OrderId = orderId, TrackingNumber = "TRACK-789" });

        Order capturedOrder = null;
        List<OrderItem> capturedItems = null;

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryMany<OrderItem>(order =>
            {
                capturedOrder = order;
                return item => item.OrderId == order.Id;
            })
            .ThenQueryOne<ShippingInfo>(items =>
            {
                capturedItems = items;
                return s => s.OrderId == capturedOrder!.Id;
            })
            .WithResult<string>(shipping =>
                $"{capturedOrder!.CustomerName}|{capturedItems!.Count}|{shipping.TrackingNumber}");

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal("Alice|1|TRACK-789", result);
    }

    #endregion

    #region Hooks

    [Fact]
    public async Task Hooks_ExecuteInOrder()
    {
        var callOrder = new List<string>();
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithResult<string>(order => order.CustomerName)
            .WithBeforeExecution(() => callOrder.Add("before"))
            .WithAfterExecution(() => callOrder.Add("after"));

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal("Alice", result);
        Assert.Equal(["before", "after"], callOrder);
    }

    [Fact]
    public async Task AsyncHooks_ExecuteInOrder()
    {
        var callOrder = new List<string>();
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Bob" });

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithResult<string>(order => order.CustomerName)
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

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal("Bob", result);
        Assert.Equal(["before-async", "after-async"], callOrder);
    }

    [Fact]
    public void NoHooks_ShouldBeNull()
    {
        var flow = new QueryPipelineFlow();
        IQueryPipelineFlowBuilder<string> builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == Guid.NewGuid())
            .WithResult<string>(order => order.CustomerName);

        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    #endregion

    #region Error Propagation

    [Fact]
    public async Task ErrorInStep1_StopsExecution()
    {
        var provider = new InMemoryQueryServiceProvider();

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == Guid.NewGuid())
            .WithErrorIfNull(new Error("Order not found"))
            .ThenQueryMany<OrderItem>(order => item => item.OrderId == order.Id)
            .WithResult<int>(items => items.Count);

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("Order not found", ex.Message);
        Assert.Equal(["GetOne:Order"], provider.Operations); // Step 2 never executed
    }

    [Fact]
    public async Task ErrorInStep2_AfterStep1Succeeds()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        // No ShippingInfo seeded

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .ThenQueryOne<ShippingInfo>(order => s => s.OrderId == order.Id)
            .WithErrorIfNull(new Error("No shipping info"))
            .WithResult<string>(s => s.TrackingNumber);

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("No shipping info", ex.Message);
        Assert.Equal(["GetOne:Order", "GetOne:ShippingInfo"], provider.Operations);
    }

    [Fact]
    public async Task NoErrorIfNull_NullPropagates()
    {
        var provider = new InMemoryQueryServiceProvider();

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == Guid.NewGuid())
            // No WithErrorIfNull — null will propagate
            .WithResult<string>(order => order?.CustomerName ?? "null");

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("null", result);
    }

    #endregion

    #region Complex Result Types

    [Fact]
    public async Task Result_ComplexObjectComposition()
    {
        var orderId = Guid.NewGuid();
        var provider = new InMemoryQueryServiceProvider();
        provider.Seed(new Order { Id = orderId, CustomerName = "Alice" });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Widget", Quantity = 3 });
        provider.Seed(new OrderItem { OrderId = orderId, Product = "Gadget", Quantity = 1 });

        Order capturedOrder = null;

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == orderId)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryMany<OrderItem>(order =>
            {
                capturedOrder = order;
                return item => item.OrderId == order.Id;
            })
            .WithResult<Dictionary<string, object>>(items => new Dictionary<string, object>
            {
                ["customer"] = capturedOrder!.CustomerName,
                ["itemCount"] = items.Count,
                ["totalQuantity"] = items.Sum(i => i.Quantity)
            });

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal("Alice", result["customer"]);
        Assert.Equal(2, result["itemCount"]);
        Assert.Equal(4, result["totalQuantity"]);
    }

    #endregion

    #region Interface Reflection

    [Fact]
    public void IQueryPipelineOneStep_ShouldInherit_IQueryPipelineNextable()
    {
        var interfaces = typeof(IQueryPipelineOneStep<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryPipelineNextable<>));
    }

    [Fact]
    public void IQueryPipelineManyStep_ShouldInherit_IQueryPipelineNextable()
    {
        var interfaces = typeof(IQueryPipelineManyStep<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryPipelineNextable<>));
    }

    [Fact]
    public void IQueryPipelineResultTerminal_ShouldInherit_IQueryPipelineFlowBuilder()
    {
        var interfaces = typeof(IQueryPipelineResultTerminal<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryPipelineFlowBuilder<>));
    }

    [Fact]
    public void IQueryPipelineNextable_ShouldHave_AllTransitionMethods()
    {
        var methods = typeof(IQueryPipelineNextable<>).GetMethods().Select(m => m.Name).ToHashSet();
        Assert.Contains("ThenQueryOne", methods);
        Assert.Contains("ThenQueryMany", methods);
        Assert.Contains("ThenQueryOneFromQueryable", methods);
        Assert.Contains("ThenQueryManyFromQueryable", methods);
        Assert.Contains("WithResult", methods);
    }

    [Fact]
    public void StepCount_MatchesChainLength()
    {
        var flow = new QueryPipelineFlow();
        IQueryPipelineFlowBuilder<int> builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == Guid.NewGuid())
            .ThenQueryMany<OrderItem>(order => item => item.OrderId == order.Id)
            .ThenQueryOne<Inventory>(items => inv => true)
            .WithResult<int>(_ => 0);

        Assert.Equal(3, builder.Steps.Count);
    }

    #endregion
}
