using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.Errors;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class QueryFlowTests
{
    private sealed class TestModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class TestResponse
    {
        public string DisplayName { get; set; }
    }

    #region QueryOne - Flow Building

    [Fact]
    public void QueryOne_SingleFilter_ShouldBuild()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        Assert.NotNull(builder.NullError);
    }

    [Fact]
    public void QueryOne_ChainedFilters_ShouldBuild()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        Assert.NotNull(builder.NullError);
    }

    [Fact]
    public void QueryOne_ThreeChainedFilters_ShouldBuild()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithFilter(x => x.Name != null)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        Assert.NotNull(builder.NullError);
    }

    #endregion

    #region QueryOne - Before/After Hooks

    [Fact]
    public void QueryOne_WithBeforeExecution_ShouldSet()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { })
            .WithErrorIfNull(new Error("Not found"));

        Assert.NotNull(builder.BeforeExecutionFunc);
    }

    [Fact]
    public async Task QueryOne_WithBeforeExecution_ShouldInvoke()
    {
        var called = false;
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { called = true; })
            .WithErrorIfNull(new Error("Not found"));

        await builder.BeforeExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public async Task QueryOne_WithBeforeExecutionAsync_ShouldInvoke()
    {
        var called = false;
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(async () =>
            {
                await Task.Delay(1);
                called = true;
            })
            .WithErrorIfNull(new Error("Not found"));

        await builder.BeforeExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public void QueryOne_WithAfterExecution_ShouldSet()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"))
            .WithAfterExecution(_ => { });

        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task QueryOne_WithAfterExecution_ShouldReceiveResponse()
    {
        TestResponse capturedResponse = null;
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"))
            .WithAfterExecution(r => { capturedResponse = r; });

        var response = new TestResponse { DisplayName = "Test" };
        await builder.AfterExecutionFunc(response);
        Assert.Same(response, capturedResponse);
    }

    [Fact]
    public void QueryOne_WithToModelPath_BeforeAndAfterHooks()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithMap(m => new TestResponse { DisplayName = m.Name })
            .WithBeforeExecution(() => { })
            .WithErrorIfNull(new Error("Not found"))
            .WithAfterExecution(_ => { });

        Assert.NotNull(builder.BeforeExecutionFunc);
        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public void QueryOne_NoHooks_ShouldBeNull()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    #endregion

    #region QueryMany - Flow Building

    [Fact]
    public void QueryMany_SingleFilter_ShouldBuild()
    {
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        Assert.NotNull(builder);
    }

    [Fact]
    public void QueryMany_ChainedFilters_ShouldBuild()
    {
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithFilter(x => x.Age >= 18)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        Assert.NotNull(builder);
    }

    [Fact]
    public void QueryMany_ThreeChainedFilters_ShouldBuild()
    {
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithFilter(x => x.Age >= 18)
            .WithFilter(x => x.Name.StartsWith("J"))
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        Assert.NotNull(builder);
    }

    #endregion

    #region QueryMany - Before/After Hooks

    [Fact]
    public void QueryMany_WithBeforeExecution_ShouldSet()
    {
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { })
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        Assert.NotNull(builder.BeforeExecutionFunc);
    }

    [Fact]
    public async Task QueryMany_WithBeforeExecution_ShouldInvoke()
    {
        var called = false;
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { called = true; })
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        await builder.BeforeExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public void QueryMany_WithAfterExecution_ShouldSet()
    {
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id))
            .WithAfterExecution(() => { });

        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task QueryMany_WithAfterExecution_ShouldInvoke()
    {
        var called = false;
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id))
            .WithAfterExecution(() => { called = true; });

        await builder.AfterExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public void QueryMany_NoHooks_ShouldBeNull()
    {
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task QueryMany_FullChain_WithHooks()
    {
        var callOrder = new List<string>();
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => callOrder.Add("before"))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id))
            .WithAfterExecution(() => callOrder.Add("after"));

        await builder.BeforeExecutionFunc();
        await builder.AfterExecutionFunc();

        Assert.Equal(["before", "after"], callOrder);
    }

    #endregion

    #region Counting - Flow Building

    [Fact]
    public void Counting_SingleFilter_ShouldBuild()
    {
        var start = new CountingFlowStart();

        var builder = (ICountingFlowBuilder)start
            .WithFilter<TestModel>(x => x.IsActive);

        Assert.NotNull(builder);
    }

    [Fact]
    public void Counting_ChainedFilters_ShouldBuild()
    {
        var start = new CountingFlowStart();

        var builder = (ICountingFlowBuilder)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithFilter(x => x.Age > 21);

        Assert.NotNull(builder);
    }

    [Fact]
    public void Counting_ThreeChainedFilters_ShouldBuild()
    {
        var start = new CountingFlowStart();

        var builder = (ICountingFlowBuilder)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithFilter(x => x.Id != null);

        Assert.NotNull(builder);
    }

    #endregion

    #region Backward Compatibility

    [Fact]
    public void BackwardCompat_QueryOne_SingleFilterDirectToSpecialAction()
    {
        var start = new QueryOneFlowStart<TestResponse>();

        var builder = (IQueryOneFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        Assert.NotNull(builder.NullError);
        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    [Fact]
    public void BackwardCompat_QueryMany_SingleFilterDirectToSpecialAction()
    {
        var start = new QueryListFlowStart<TestResponse>();

        var builder = (IQueryListFlowBuilder<TestResponse>)start
            .WithFilter<TestModel>(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    [Fact]
    public void BackwardCompat_Counting_SingleFilterDirectToBuilder()
    {
        var start = new CountingFlowStart();

        var builder = (ICountingFlowBuilder)start
            .WithFilter<TestModel>(x => x.IsActive);

        Assert.NotNull(builder);
    }

    #endregion

    #region Interface Reflection Tests

    [Fact]
    public void IQueryOneAfterFilter_ShouldExist()
    {
        Assert.True(typeof(IQueryOneAfterFilter<,>).IsInterface);
    }

    [Fact]
    public void IQueryOneAfterFilter_ShouldInherit_IQueryOneSpecialAction()
    {
        var interfaces = typeof(IQueryOneAfterFilter<,>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryOneSpecialAction<,>));
    }

    [Fact]
    public void IQueryOneAfterFilter_ShouldDefine_WithFilter()
    {
        var methods = typeof(IQueryOneAfterFilter<,>).GetMethods()
            .Where(m => m.DeclaringType == typeof(IQueryOneAfterFilter<,>))
            .Select(m => m.Name).ToList();
        Assert.Contains("WithFilter", methods);
    }

    [Fact]
    public void IQueryOneAfterBuild_ShouldInherit_IQueryOneFlowBuilder()
    {
        var interfaces = typeof(IQueryOneAfterBuild<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryOneFlowBuilder<>));
    }

    [Fact]
    public void IQueryOneAfterBuild_ShouldDefine_WithAfterExecution()
    {
        var methods = typeof(IQueryOneAfterBuild<>).GetMethods()
            .Where(m => m.DeclaringType == typeof(IQueryOneAfterBuild<>))
            .Select(m => m.Name).ToList();
        Assert.Contains("WithAfterExecution", methods);
    }

    [Fact]
    public void IQueryOneFlowBuilder_ShouldDefine_HookProperties()
    {
        var type = typeof(IQueryOneFlowBuilder<>);
        Assert.NotNull(type.GetProperty("BeforeExecutionFunc"));
        Assert.NotNull(type.GetProperty("AfterExecutionFunc"));
    }

    [Fact]
    public void IQueryListAfterFilter_ShouldInherit_IQueryListSpecialAction()
    {
        var interfaces = typeof(IQueryListAfterFilter<,>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryListSpecialAction<,>));
    }

    [Fact]
    public void IQueryListAfterBuild_ShouldInherit_IQueryListFlowBuilder()
    {
        var interfaces = typeof(IQueryListAfterBuild<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryListFlowBuilder<>));
    }

    [Fact]
    public void ICountingAfterFilter_ShouldInherit_ICountingFlowBuilder()
    {
        var interfaces = typeof(ICountingAfterFilter<>).GetInterfaces();
        Assert.Contains(interfaces, i => i == typeof(ICountingFlowBuilder));
    }

    [Fact]
    public void IQueryListFlowBuilder_ShouldDefine_HookProperties()
    {
        var type = typeof(IQueryListFlowBuilder<>);
        Assert.NotNull(type.GetProperty("BeforeExecutionFunc"));
        Assert.NotNull(type.GetProperty("AfterExecutionFunc"));
    }

    #endregion
}
