using FlowX.Abstractions.RequestFlow.Queries.QueryFlow;
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

    #region QueryOne - Chainable Filters

    [Fact]
    public void QueryOne_SingleFilter_ShouldSetFilter()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        var builder = ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        var result = (IQueryOneFlowBuilder<TestModel, TestResponse>)builder;
        Assert.NotNull(result.Filter);
    }

    [Fact]
    public void QueryOne_ChainedFilters_ShouldComposeWithAnd()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        var builder = ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        var result = (IQueryOneFlowBuilder<TestModel, TestResponse>)builder;
        Assert.NotNull(result.Filter);

        // Compile and test the composed filter
        var compiledFilter = result.Filter.Compile();
        Assert.True(compiledFilter(new TestModel { IsActive = true, Age = 25 }));
        Assert.False(compiledFilter(new TestModel { IsActive = false, Age = 25 }));
        Assert.False(compiledFilter(new TestModel { IsActive = true, Age = 10 }));
        Assert.False(compiledFilter(new TestModel { IsActive = false, Age = 10 }));
    }

    [Fact]
    public void QueryOne_ThreeChainedFilters_ShouldComposeAll()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        var builder = ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithFilter(x => x.Name != null)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        var result = (IQueryOneFlowBuilder<TestModel, TestResponse>)builder;
        var compiledFilter = result.Filter.Compile();

        Assert.True(compiledFilter(new TestModel { IsActive = true, Age = 25, Name = "John" }));
        Assert.False(compiledFilter(new TestModel { IsActive = true, Age = 25, Name = null }));
    }

    #endregion

    #region QueryOne - Before/After Hooks

    [Fact]
    public void QueryOne_WithBeforeExecution_ShouldSet()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { })
            .WithErrorIfNull(new Error("Not found"));

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.BeforeExecutionFunc);
    }

    [Fact]
    public async Task QueryOne_WithBeforeExecution_ShouldInvoke()
    {
        var called = false;
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { called = true; })
            .WithErrorIfNull(new Error("Not found"));

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        await builder.BeforeExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public async Task QueryOne_WithBeforeExecutionAsync_ShouldInvoke()
    {
        var called = false;
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(async () =>
            {
                await Task.Delay(1);
                called = true;
            })
            .WithErrorIfNull(new Error("Not found"));

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        await builder.BeforeExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public void QueryOne_WithAfterExecution_ShouldSet()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"))
            .WithAfterExecution(_ => { });

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task QueryOne_WithAfterExecution_ShouldReceiveResponse()
    {
        TestResponse capturedResponse = null;
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"))
            .WithAfterExecution(r => { capturedResponse = r; });

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        var response = new TestResponse { DisplayName = "Test" };
        await builder.AfterExecutionFunc(response);
        Assert.Same(response, capturedResponse);
    }

    [Fact]
    public void QueryOne_WithToModelPath_BeforeAndAfterHooks()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithMap(m => new TestResponse { DisplayName = m.Name })
            .WithBeforeExecution(() => { })
            .WithErrorIfNull(new Error("Not found"))
            .WithAfterExecution(_ => { });

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.BeforeExecutionFunc);
        Assert.NotNull(builder.AfterExecutionFunc);
        Assert.Equal(QuerySpecialActionType.ToModel, builder.QuerySpecialActionType);
    }

    [Fact]
    public void QueryOne_NoHooks_ShouldBeNull()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    #endregion

    #region QueryMany - Chainable Filters

    [Fact]
    public void QueryMany_SingleFilter_ShouldSetFilter()
    {
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.Filter);
    }

    [Fact]
    public void QueryMany_ChainedFilters_ShouldComposeWithAnd()
    {
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithFilter(x => x.Age >= 18)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        var compiledFilter = builder.Filter.Compile();

        Assert.True(compiledFilter(new TestModel { IsActive = true, Age = 18 }));
        Assert.False(compiledFilter(new TestModel { IsActive = true, Age = 17 }));
        Assert.False(compiledFilter(new TestModel { IsActive = false, Age = 18 }));
    }

    [Fact]
    public void QueryMany_ThreeChainedFilters_ShouldComposeAll()
    {
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithFilter(x => x.Age >= 18)
            .WithFilter(x => x.Name.StartsWith("J"))
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        var compiledFilter = builder.Filter.Compile();

        Assert.True(compiledFilter(new TestModel { IsActive = true, Age = 25, Name = "John" }));
        Assert.False(compiledFilter(new TestModel { IsActive = true, Age = 25, Name = "Bob" }));
    }

    #endregion

    #region QueryMany - Before/After Hooks

    [Fact]
    public void QueryMany_WithBeforeExecution_ShouldSet()
    {
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { })
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.BeforeExecutionFunc);
    }

    [Fact]
    public async Task QueryMany_WithBeforeExecution_ShouldInvoke()
    {
        var called = false;
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => { called = true; })
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        await builder.BeforeExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public void QueryMany_WithAfterExecution_ShouldSet()
    {
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id))
            .WithAfterExecution(() => { });

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task QueryMany_WithAfterExecution_ShouldInvoke()
    {
        var called = false;
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id))
            .WithAfterExecution(() => { called = true; });

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        await builder.AfterExecutionFunc();
        Assert.True(called);
    }

    [Fact]
    public void QueryMany_NoHooks_ShouldBeNull()
    {
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task QueryMany_FullChain_WithHooks()
    {
        var callOrder = new List<string>();
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithBeforeExecution(() => callOrder.Add("before"))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id))
            .WithAfterExecution(() => callOrder.Add("after"));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        await builder.BeforeExecutionFunc();
        await builder.AfterExecutionFunc();

        Assert.Equal(["before", "after"], callOrder);
        Assert.NotNull(builder.Filter);
        Assert.Equal(QuerySpecialActionType.ToTarget, builder.QuerySpecialActionType);
    }

    #endregion

    #region Counting - Chainable Filters

    [Fact]
    public void Counting_SingleFilter_ShouldSetFilter()
    {
        var flow = new CountingFlow<TestModel>();

        var builder = ((ICountingFilter<TestModel>)flow)
            .WithFilter(x => x.IsActive);

        var result = (ICountingFlowBuilder<TestModel>)builder;
        Assert.NotNull(result.Filter);
    }

    [Fact]
    public void Counting_ChainedFilters_ShouldComposeWithAnd()
    {
        var flow = new CountingFlow<TestModel>();

        var builder = ((ICountingFilter<TestModel>)flow)
            .WithFilter(x => x.IsActive)
            .WithFilter(x => x.Age > 21);

        var result = (ICountingFlowBuilder<TestModel>)builder;
        var compiledFilter = result.Filter.Compile();

        Assert.True(compiledFilter(new TestModel { IsActive = true, Age = 25 }));
        Assert.False(compiledFilter(new TestModel { IsActive = false, Age = 25 }));
        Assert.False(compiledFilter(new TestModel { IsActive = true, Age = 20 }));
    }

    [Fact]
    public void Counting_ThreeChainedFilters_ShouldComposeAll()
    {
        var flow = new CountingFlow<TestModel>();

        var builder = ((ICountingFilter<TestModel>)flow)
            .WithFilter(x => x.IsActive)
            .WithFilter(x => x.Age > 18)
            .WithFilter(x => x.Id != null);

        var result = (ICountingFlowBuilder<TestModel>)builder;
        var compiledFilter = result.Filter.Compile();

        Assert.True(compiledFilter(new TestModel { IsActive = true, Age = 25, Id = "1" }));
        Assert.False(compiledFilter(new TestModel { IsActive = true, Age = 25, Id = null }));
    }

    #endregion

    #region Backward Compatibility

    [Fact]
    public void BackwardCompat_QueryOne_SingleFilterDirectToSpecialAction()
    {
        var flow = new QueryOneFlow<TestModel, TestResponse>();

        ((IQueryOneFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.Id == "1")
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithErrorIfNull(new Error("Not found"));

        var builder = (IQueryOneFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.Filter);
        Assert.Equal(QuerySpecialActionType.ToTarget, builder.QuerySpecialActionType);
        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    [Fact]
    public void BackwardCompat_QueryMany_SingleFilterDirectToSpecialAction()
    {
        var flow = new QueryManyFlow<TestModel, TestResponse>();

        ((IQueryListFilter<TestModel, TestResponse>)flow)
            .WithFilter(x => x.IsActive)
            .WithSpecialAction(q => q.Select(m => new TestResponse { DisplayName = m.Name }))
            .WithDefaultSortFields(ExpressionOrder<TestModel>.Of(x => x.Id));

        var builder = (IQueryListFlowBuilder<TestModel, TestResponse>)flow;
        Assert.NotNull(builder.Filter);
        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    [Fact]
    public void BackwardCompat_Counting_SingleFilterDirectToBuilder()
    {
        var flow = new CountingFlow<TestModel>();

        var builder = (ICountingFlowBuilder<TestModel>)((ICountingFilter<TestModel>)flow)
            .WithFilter(x => x.IsActive);

        Assert.NotNull(builder.Filter);
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
        var interfaces = typeof(IQueryOneAfterBuild<,>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryOneFlowBuilder<,>));
    }

    [Fact]
    public void IQueryOneAfterBuild_ShouldDefine_WithAfterExecution()
    {
        var methods = typeof(IQueryOneAfterBuild<,>).GetMethods()
            .Where(m => m.DeclaringType == typeof(IQueryOneAfterBuild<,>))
            .Select(m => m.Name).ToList();
        Assert.Contains("WithAfterExecution", methods);
    }

    [Fact]
    public void IQueryOneFlowBuilder_ShouldDefine_HookProperties()
    {
        var type = typeof(IQueryOneFlowBuilder<,>);
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
        var interfaces = typeof(IQueryListAfterBuild<,>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryListFlowBuilder<,>));
    }

    [Fact]
    public void ICountingAfterFilter_ShouldInherit_ICountingFlowBuilder()
    {
        var interfaces = typeof(ICountingAfterFilter<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICountingFlowBuilder<>));
    }

    [Fact]
    public void IQueryListFlowBuilder_ShouldDefine_HookProperties()
    {
        var type = typeof(IQueryListFlowBuilder<,>);
        Assert.NotNull(type.GetProperty("BeforeExecutionFunc"));
        Assert.NotNull(type.GetProperty("AfterExecutionFunc"));
    }

    #endregion
}
