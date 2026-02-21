using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;
using FlowX.Errors;
using FlowX.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class QueryPipelineEfCoreTests
{
    // === EF Core Models ===

    private sealed class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    private sealed class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    private sealed class Inventory
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
    }

    private sealed class ShippingInfo
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
    }

    // === Test DbContext ===

    private sealed class PipelineTestDbContext(DbContextOptions<PipelineTestDbContext> options)
        : DbContext(options)
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<ShippingInfo> ShippingInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(e => e.HasKey(o => o.Id));
            modelBuilder.Entity<OrderItem>(e => e.HasKey(o => o.Id));
            modelBuilder.Entity<Inventory>(e => e.HasKey(o => o.Id));
            modelBuilder.Entity<ShippingInfo>(e => e.HasKey(o => o.Id));
            base.OnModelCreating(modelBuilder);
        }
    }

    // === EF Core Service Provider (mirrors EfQueryPipelineServiceProvider) ===

    private sealed class EfTestQueryServiceProvider(PipelineTestDbContext dbContext) : IQueryPipelineServiceProvider
    {
        public async Task<TModel> GetFirstByConditionAsync<TModel>(
            Expression<Func<TModel, bool>> filter,
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
            CancellationToken ct) where TModel : class
        {
            var queryable = dbContext.Set<TModel>().AsNoTracking().AsQueryable();
            if (filter is not null)
                queryable = queryable.Where(filter);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return await queryable.FirstOrDefaultAsync(ct);
        }

        public async Task<List<TModel>> GetManyByConditionAsync<TModel>(
            Expression<Func<TModel, bool>> filter,
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
            CancellationToken ct) where TModel : class
        {
            var queryable = dbContext.Set<TModel>().AsNoTracking().AsQueryable();
            if (filter is not null)
                queryable = queryable.Where(filter);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return await queryable.ToListAsync(ct);
        }

        public async Task<QueryPipelinePage<TModel>> GetManyWithPaginationAsync<TModel>(
            Expression<Func<TModel, bool>> filter,
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
            ExpressionOrder<TModel> defaultSort,
            string sortedFields,
            int? skip, int? take,
            CancellationToken ct) where TModel : class
        {
            var queryable = dbContext.Set<TModel>().AsNoTracking().AsQueryable();
            if (filter is not null)
                queryable = queryable.Where(filter);

            var sortExpressions = defaultSort?.ExpressionDetails;
            var ordered = queryable.OrderDynamicOrDefault(sortedFields, sortExpressions);

            var finalQueryable = specialAction is not null ? specialAction(ordered) : ordered;

            var totalRecord = await finalQueryable.LongCountAsync(ct);
            var items = await finalQueryable.Offset(skip).Limit(take).ToListAsync(ct);

            return new QueryPipelinePage<TModel> { Items = items, TotalRecord = totalRecord };
        }

        public async Task<long> GetCountAsync<TModel>(
            Expression<Func<TModel, bool>> filter,
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction,
            CancellationToken ct) where TModel : class
        {
            var queryable = dbContext.Set<TModel>().AsNoTracking().AsQueryable();
            if (filter is not null)
                queryable = queryable.Where(filter);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return await queryable.LongCountAsync(ct);
        }
    }

    // === Helpers ===

    private static PipelineTestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PipelineTestDbContext>()
            .UseInMemoryDatabase($"PipelineTest_{Guid.NewGuid()}")
            .Options;
        return new PipelineTestDbContext(options);
    }

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

    // === Seed Data ===

    private static readonly Guid OrderId1 = Guid.NewGuid();
    private static readonly Guid OrderId2 = Guid.NewGuid();
    private static readonly Guid OrderId3 = Guid.NewGuid();

    private static async Task SeedData(PipelineTestDbContext db)
    {
        db.Orders.AddRange(
            new Order
            {
                Id = OrderId1, CustomerName = "Alice", TotalAmount = 250m, IsActive = true,
                CreatedAt = new DateTime(2024, 1, 15)
            },
            new Order
            {
                Id = OrderId2, CustomerName = "Bob", TotalAmount = 100m, IsActive = true,
                CreatedAt = new DateTime(2024, 2, 10)
            },
            new Order
            {
                Id = OrderId3, CustomerName = "Charlie", TotalAmount = 500m, IsActive = false,
                CreatedAt = new DateTime(2024, 3, 5)
            });

        db.OrderItems.AddRange(
            new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = OrderId1, ProductName = "Widget", Quantity = 5, UnitPrice = 30m
            },
            new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = OrderId1, ProductName = "Gadget", Quantity = 2, UnitPrice = 50m
            },
            new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = OrderId2, ProductName = "Widget", Quantity = 1, UnitPrice = 30m
            },
            new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = OrderId2, ProductName = "Doohickey", Quantity = 3, UnitPrice = 20m
            },
            new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = OrderId3, ProductName = "Thingamajig", Quantity = 10, UnitPrice = 50m
            });

        db.Inventories.AddRange(
            new Inventory { Id = Guid.NewGuid(), ProductName = "Widget", Stock = 100 },
            new Inventory { Id = Guid.NewGuid(), ProductName = "Gadget", Stock = 50 },
            new Inventory { Id = Guid.NewGuid(), ProductName = "Doohickey", Stock = 200 },
            new Inventory { Id = Guid.NewGuid(), ProductName = "Thingamajig", Stock = 10 });

        db.ShippingInfos.AddRange(
            new ShippingInfo
            {
                Id = Guid.NewGuid(), OrderId = OrderId1, TrackingNumber = "TRACK-001", Carrier = "FedEx"
            },
            new ShippingInfo
            {
                Id = Guid.NewGuid(), OrderId = OrderId2, TrackingNumber = "TRACK-002", Carrier = "UPS"
            });

        await db.SaveChangesAsync();
    }

    #region Single Step — QueryOne with EF Core

    [Fact]
    public async Task EfCore_QueryOne_ByFilter()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithResult<string>(order => order.CustomerName);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Alice", result);
    }

    [Fact]
    public async Task EfCore_QueryOne_WithSpecialAction_ActiveOnly()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId3)
            .WithSpecialAction(q => q.Where(o => o.IsActive))
            .WithResult<string>(order => order?.CustomerName ?? "not found");

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("not found", result); // OrderId3 is inactive
    }

    [Fact]
    public async Task EfCore_QueryOne_WithErrorIfNull_Throws()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == Guid.NewGuid())
            .WithErrorIfNull(new Error("Order not found"))
            .WithResult<string>(order => order.CustomerName);

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("Order not found", ex.Message);
    }

    #endregion

    #region Single Step — QueryMany with EF Core

    [Fact]
    public async Task EfCore_QueryMany_WithFilter()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(item => item.OrderId == OrderId1)
            .WithResult<int>(items => items.Count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(2, result); // Widget + Gadget
    }

    [Fact]
    public async Task EfCore_QueryMany_WithSpecialAction_FilterQuantity()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(_ => true)
            .WithSpecialAction(q => q.Where(i => i.Quantity >= 3))
            .WithResult<List<string>>(items => items.Select(i => i.ProductName).ToList());

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(3, result.Count); // Widget(5), Doohickey(3), Thingamajig(10)
    }

    [Fact]
    public async Task EfCore_QueryMany_WithDefaultSortFields()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        // Pattern giống GetProvincesHandler:
        // .WithDefaultSortFields(Asc(a => a.Name).ThenDescBy(x => x.Id))
        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<Order>(o => o.IsActive)
            .WithDefaultSortFields(ExpressionOrder<Order>.Of(o => o.CustomerName))
            .WithResult<List<string>>(orders => orders.Select(o => o.CustomerName).ToList());

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(["Alice", "Bob"], result);
    }

    [Fact]
    public async Task EfCore_QueryMany_WithDefaultSortFields_Descending()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<Order>(_ => true)
            .WithDefaultSortFields(ExpressionOrder<Order>.Of(o => o.TotalAmount, isAsc: false))
            .WithResult<List<decimal>>(orders => orders.Select(o => o.TotalAmount).ToList());

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal([500m, 250m, 100m], result);
    }

    #endregion

    #region Multi-Step — QueryOne then QueryMany (like GetOrder → GetOrderItems)

    [Fact]
    public async Task EfCore_QueryOne_ThenQueryMany_FilterBased()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Order not found"))
            .ThenQueryMany<OrderItem>(order => item => item.OrderId == order.Id)
            .WithResult<decimal>(items => items.Sum(i => i.Quantity * i.UnitPrice));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(5 * 30m + 2 * 50m, result); // 150 + 100 = 250
    }

    [Fact]
    public async Task EfCore_QueryOne_ThenQueryOne_ShippingInfo()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Order not found"))
            .ThenQueryOne<ShippingInfo>(order => s => s.OrderId == order.Id)
            .WithErrorIfNull(new Error("No shipping info"))
            .WithResult<string>(s => $"{s.Carrier}: {s.TrackingNumber}");

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("FedEx: TRACK-001", result);
    }

    [Fact]
    public async Task EfCore_QueryOne_ThenQueryOne_NoShipping_ThrowsError()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        // OrderId3 (Charlie) has no shipping info
        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId3)
            .ThenQueryOne<ShippingInfo>(order => s => s.OrderId == order.Id)
            .WithErrorIfNull(new Error("No shipping info for this order"))
            .WithResult<string>(s => s.TrackingNumber);

        var ex = await Assert.ThrowsAsync<Error>(() => ExecutePipeline(builder, provider));
        Assert.Equal("No shipping info for this order", ex.Message);
    }

    #endregion

    #region Multi-Step — Queryable-based Transitions with EF Core

    [Fact]
    public async Task EfCore_ThenQueryManyFromQueryable_WithComplexFilter()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Order not found"))
            .ThenQueryManyFromQueryable<OrderItem>((order, q) =>
                q.Where(item => item.OrderId == order.Id && item.UnitPrice > 40m))
            .WithResult<List<string>>(items => items.Select(i => i.ProductName).ToList());

        var result = await ExecutePipeline(builder, provider);
        Assert.Single(result);
        Assert.Equal("Gadget", result[0]); // Only Gadget has UnitPrice=50 > 40
    }

    [Fact]
    public async Task EfCore_ThenQueryOneFromQueryable_JoinLikePattern()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        // Get order, then find inventory for a product in that order
        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(item => item.OrderId == OrderId1)
            .ThenQueryOneFromQueryable<Inventory>((items, q) =>
                q.Where(inv => inv.ProductName == items.First().ProductName))
            .WithResult<int>(inv => inv?.Stock ?? 0);

        var result = await ExecutePipeline(builder, provider);
        Assert.True(result > 0);
    }

    #endregion

    #region Three-Step Chain with EF Core

    [Fact]
    public async Task EfCore_ThreeSteps_Order_Items_Inventory()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        Order capturedOrder = null;

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId2)
            .WithErrorIfNull(new Error("Order not found"))
            .ThenQueryMany<OrderItem>(order =>
            {
                capturedOrder = order;
                return item => item.OrderId == order.Id;
            })
            .ThenQueryManyFromQueryable<Inventory>((items, q) =>
            {
                var productNames = items.Select(i => i.ProductName).ToList();
                return q.Where(inv => productNames.Contains(inv.ProductName));
            })
            .WithResult<Dictionary<string, object>>(inventories => new Dictionary<string, object>
            {
                ["customer"] = capturedOrder!.CustomerName,
                ["inventoryCount"] = inventories.Count,
                ["totalStock"] = inventories.Sum(i => i.Stock)
            });

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal("Bob", result["customer"]);
        Assert.Equal(2, result["inventoryCount"]); // Widget + Doohickey
        Assert.Equal(100 + 200, result["totalStock"]); // 300
    }

    [Fact]
    public async Task EfCore_ThreeSteps_Order_Shipping_Inventory()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryOne<ShippingInfo>(order => s => s.OrderId == order.Id)
            .WithErrorIfNull(new Error("No shipping"))
            .ThenQueryMany<Inventory>(_ => inv => inv.Stock > 30)
            .WithResult<int>(inventories => inventories.Count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(3, result); // Widget(100), Gadget(50), Doohickey(200)
    }

    #endregion

    #region Pagination with EF Core

    [Fact]
    public async Task EfCore_QueryPaginated_BasicPaging()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        // Similar to GetProvincesHandler pattern but with pagination
        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryPaginated<Order>(_ => true, skip: 0, take: 2)
            .WithDefaultSortFields(ExpressionOrder<Order>.Of(o => o.CustomerName))
            .WithResult<(List<string> names, long total)>(page =>
                (page.Items.Select(o => o.CustomerName).ToList(), page.TotalRecord));

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal(2, result.names.Count);
        Assert.Equal(3, result.total);
        Assert.Equal("Alice", result.names[0]);
        Assert.Equal("Bob", result.names[1]);
    }

    [Fact]
    public async Task EfCore_QueryPaginated_SecondPage()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryPaginated<Order>(_ => true, skip: 2, take: 2)
            .WithDefaultSortFields(ExpressionOrder<Order>.Of(o => o.CustomerName))
            .WithResult<(List<string> names, long total)>(page =>
                (page.Items.Select(o => o.CustomerName).ToList(), page.TotalRecord));

        var result = await ExecutePipeline(builder, provider);

        Assert.Single(result.names);
        Assert.Equal(3, result.total);
        Assert.Equal("Charlie", result.names[0]);
    }

    [Fact]
    public async Task EfCore_QueryPaginated_WithFilter_ActiveOnly()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryPaginated<Order>(o => o.IsActive, skip: 0, take: 10)
            .WithDefaultSortFields(ExpressionOrder<Order>.Of(o => o.CustomerName))
            .WithResult<(int count, long total)>(page =>
                (page.Items.Count, page.TotalRecord));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(2, result.count);
        Assert.Equal(2, result.total);
    }

    [Fact]
    public async Task EfCore_QueryPaginated_WithSortedFieldsString()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        // Dynamic sort string like "TotalAmount desc" — similar to SortedFields from GetManyQuery
        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryPaginated<Order>(_ => true, skip: 0, take: 10, sortedFields: "TotalAmount desc")
            .WithDefaultSortFields(ExpressionOrder<Order>.Of(o => o.CustomerName))
            .WithResult<List<string>>(page =>
                page.Items.Select(o => o.CustomerName).ToList());

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(["Charlie", "Alice", "Bob"], result); // 500, 250, 100 desc
    }

    [Fact]
    public async Task EfCore_QueryPaginated_WithSpecialAction()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryPaginated<OrderItem>(_ => true, skip: 0, take: 3)
            .WithSpecialAction(q => q.Where(i => i.Quantity >= 3))
            .WithDefaultSortFields(ExpressionOrder<OrderItem>.Of(i => i.ProductName))
            .WithResult<(List<string> products, long total)>(page =>
                (page.Items.Select(i => i.ProductName).ToList(), page.TotalRecord));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(3, result.total); // Doohickey(3), Thingamajig(10), Widget(5)
        Assert.Equal(3, result.products.Count);
    }

    #endregion

    #region Multi-Step with Pagination Transition

    [Fact]
    public async Task EfCore_QueryOne_ThenQueryPaginated()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Order not found"))
            .ThenQueryPaginated<OrderItem>(
                order => item => item.OrderId == order.Id,
                skip: 0, take: 1)
            .WithDefaultSortFields(ExpressionOrder<OrderItem>.Of(i => i.ProductName))
            .WithResult<(string firstProduct, long total)>(page =>
                (page.Items.First().ProductName, page.TotalRecord));

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal(2, result.total); // Alice has 2 items
        Assert.Equal("Gadget", result.firstProduct); // Sorted by ProductName: Gadget before Widget
    }

    [Fact]
    public async Task EfCore_QueryOne_ThenQueryPaginatedFromQueryable()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryPaginatedFromQueryable<Inventory>(
                (order, q) => q.Where(inv => inv.Stock >= 50),
                skip: 0, take: 10)
            .WithDefaultSortFields(ExpressionOrder<Inventory>.Of(i => i.ProductName))
            .WithResult<List<string>>(page =>
                page.Items.Select(i => i.ProductName).ToList());

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(["Doohickey", "Gadget", "Widget"], result); // Stock >= 50, sorted by name
    }

    [Fact]
    public async Task EfCore_QueryPaginated_ThenQueryOne_ChainFromPage()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryPaginated<Order>(o => o.IsActive, skip: 0, take: 1)
            .WithDefaultSortFields(ExpressionOrder<Order>.Of(o => o.CustomerName))
            .ThenQueryOne<ShippingInfo>(page =>
                s => s.OrderId == page.Items.First().Id)
            .WithErrorIfNull(new Error("No shipping"))
            .WithResult<string>(s => s.TrackingNumber);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("TRACK-001", result); // First active order sorted by name = Alice → TRACK-001
    }

    #endregion

    #region Hooks with EF Core

    [Fact]
    public async Task EfCore_Hooks_BeforeAndAfter()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);
        var hookLog = new List<string>();

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithResult<string>(order => order.CustomerName)
            .WithBeforeExecution(() => hookLog.Add("before"))
            .WithAfterExecution(() => hookLog.Add("after"));

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal("Alice", result);
        Assert.Equal(["before", "after"], hookLog);
    }

    #endregion

    #region Query Counting with EF Core

    [Fact]
    public async Task EfCore_QueryCounting_Basic()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryCounting<Order>(o => o.IsActive)
            .WithResult<long>(count => count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(2, result); // Alice + Bob are active
    }

    [Fact]
    public async Task EfCore_QueryCounting_WithSpecialAction()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryCounting<OrderItem>(_ => true)
            .WithSpecialAction(q => q.Where(i => i.Quantity >= 3))
            .WithResult<long>(count => count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(3, result); // Widget(5), Doohickey(3), Thingamajig(10)
    }

    [Fact]
    public async Task EfCore_QueryOne_ThenQueryCounting()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryCounting<OrderItem>(order => item => item.OrderId == order.Id)
            .WithResult<long>(count => count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(2, result); // Alice has Widget + Gadget
    }

    [Fact]
    public async Task EfCore_QueryCounting_ThenQueryOne()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryCounting<OrderItem>(i => i.OrderId == OrderId1)
            .ThenQueryOne<Order>(count => o => o.Id == OrderId1)
            .WithResult<(long count, string customer)>(order => (2, order.CustomerName));

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal("Alice", result.customer);
    }

    [Fact]
    public async Task EfCore_QueryOne_ThenQueryCountingFromQueryable()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithErrorIfNull(new Error("Not found"))
            .ThenQueryCountingFromQueryable<OrderItem>((order, q) =>
                q.Where(i => i.OrderId == order.Id && i.UnitPrice > 40m))
            .WithResult<long>(count => count);

        var result = await ExecutePipeline(builder, provider);
        Assert.Equal(1, result); // Only Gadget (UnitPrice=50)
    }

    #endregion

    #region Complex Scenarios — Real-world handler patterns

    [Fact]
    public async Task EfCore_GetOrderDetailPipeline_FullScenario()
    {
        // Simulates a handler like: GetOrderDetailHandler : EfQueryPipelineHandler<GetOrderDetailQuery, OrderDetailResponse>
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        Order capturedOrder = null;
        List<OrderItem> capturedItems = null;

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryOne<Order>(o => o.Id == OrderId1)
            .WithSpecialAction(q => q.Where(o => o.IsActive))
            .WithErrorIfNull(new Error { Code = "NotFound", Messages = ["Order was not found!"] })
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
            .WithResult<Dictionary<string, object>>(shipping => new Dictionary<string, object>
            {
                ["orderId"] = capturedOrder!.Id,
                ["customer"] = capturedOrder.CustomerName,
                ["totalAmount"] = capturedOrder.TotalAmount,
                ["itemCount"] = capturedItems!.Count,
                ["tracking"] = shipping?.TrackingNumber ?? "N/A",
                ["carrier"] = shipping?.Carrier ?? "N/A"
            });

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal(OrderId1, result["orderId"]);
        Assert.Equal("Alice", result["customer"]);
        Assert.Equal(250m, result["totalAmount"]);
        Assert.Equal(2, result["itemCount"]);
        Assert.Equal("TRACK-001", result["tracking"]);
        Assert.Equal("FedEx", result["carrier"]);
    }

    [Fact]
    public async Task EfCore_GetOrdersPaginatedPipeline_LikeGetProvincesHandler()
    {
        // Pattern giống GetProvincesHandler:
        //   .WithFilter(null)
        //   .WithSpecialAction(a => a.Select(...))
        //   .WithDefaultSortFields(Asc(a => a.Name).ThenDescBy(x => x.Id))
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryPaginated<Order>(o => o.IsActive, skip: 0, take: 10)
            .WithDefaultSortFields(
                ExpressionOrder<Order>.Of(o => o.CustomerName)
                    .ThenDescBy(o => o.Id))
            .WithResult<(List<object> items, long total)>(page => (
                page.Items.Select(o => new { o.Id, o.CustomerName, o.TotalAmount } as object).ToList(),
                page.TotalRecord));

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal(2, result.total);
        Assert.Equal(2, result.items.Count);
    }

    [Fact]
    public async Task EfCore_InventoryCheckPipeline_ManyToMany()
    {
        await using var db = CreateDbContext();
        await SeedData(db);
        var provider = new EfTestQueryServiceProvider(db);

        // Get all items for an order, then check inventory for those products
        var flow = new QueryPipelineFlow();
        var builder = ((IStartQueryPipeline)flow)
            .QueryMany<OrderItem>(item => item.OrderId == OrderId2)
            .ThenQueryManyFromQueryable<Inventory>((items, q) =>
            {
                var productNames = items.Select(i => i.ProductName).ToList();
                return q.Where(inv => productNames.Contains(inv.ProductName) && inv.Stock > 0);
            })
            .WithResult<List<(string product, int stock)>>(inventories =>
                inventories.Select(inv => (inv.ProductName, inv.Stock)).ToList());

        var result = await ExecutePipeline(builder, provider);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.product == "Widget" && r.stock == 100);
        Assert.Contains(result, r => r.product == "Doohickey" && r.stock == 200);
    }

    #endregion
}
