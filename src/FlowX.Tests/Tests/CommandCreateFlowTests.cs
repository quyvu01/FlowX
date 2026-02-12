using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.Errors;
using FlowX.Structs;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class CommandCreateFlowTests
{
    private sealed class TestModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    #region Create - Multiple Conditions

    [Fact]
    public async Task CreateOne_WithMultipleConditions_ShouldComposeInOrder()
    {
        var callOrder = new List<int>();
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => { callOrder.Add(1); return None.Value; })
            .WithCondition(_ => { callOrder.Add(2); return None.Value; })
            .WithCondition(_ => { callOrder.Add(3); return None.Value; })
            .WithErrorIfSaveChange(new Error("Save failed"));

        var result = await ((ICommandOneFlowBuilderVoid<TestModel>)flow).ConditionAsync(new TestModel());
        Assert.True(result.IsT0);
        Assert.Equal([1, 2, 3], callOrder);
    }

    [Fact]
    public async Task CreateOne_WithMultipleConditions_ShouldFailFastOnError()
    {
        var callOrder = new List<int>();
        var error = new Error("Condition 2 failed");
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => { callOrder.Add(1); return None.Value; })
            .WithCondition(_ => { callOrder.Add(2); return error; })
            .WithCondition(_ => { callOrder.Add(3); return None.Value; })
            .WithErrorIfSaveChange(new Error("Save failed"));

        var result = await ((ICommandOneFlowBuilderVoid<TestModel>)flow).ConditionAsync(new TestModel());
        Assert.True(result.IsT1);
        Assert.Same(error, result.AsT1);
        Assert.Equal([1, 2], callOrder);
    }

    #endregion

    #region Create - WithModify

    [Fact]
    public async Task CreateOne_WithModify_ShouldEnrichModel()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithModify(m => m.CreatedAt = now)
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        var model = new TestModel();
        await builder.CreateModifyFunc(model);
        Assert.Equal(now, model.CreatedAt);
    }

    #endregion

    #region Create - Before/After Hooks

    [Fact]
    public void CreateOne_WithBeforeExecution_FromAfterCondition_ShouldSet()
    {
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithBeforeExecution(_ => { })
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        Assert.NotNull(builder.BeforeExecutionFunc);
        Assert.Null(builder.CreateModifyFunc);
    }

    [Fact]
    public void CreateOne_WithBeforeExecution_AfterModify_ShouldSet()
    {
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithModify(m => m.Name = "enriched")
            .WithBeforeExecution(_ => { })
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        Assert.NotNull(builder.BeforeExecutionFunc);
        Assert.NotNull(builder.CreateModifyFunc);
    }

    [Fact]
    public async Task CreateOne_WithBeforeExecution_ShouldInvokeAction()
    {
        var called = false;
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithBeforeExecution(_ => { called = true; })
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        await builder.BeforeExecutionFunc(new TestModel());
        Assert.True(called);
    }

    [Fact]
    public void CreateOne_WithAfterExecution_ShouldSet()
    {
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(_ => { });

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task CreateOne_WithAfterExecution_ShouldInvokeAction()
    {
        string capturedId = null;
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(m => { capturedId = m.Id; });

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        await builder.AfterExecutionFunc(new TestModel { Id = "after-id" });
        Assert.Equal("after-id", capturedId);
    }

    [Fact]
    public async Task CreateOne_FullChain_ShouldBuildAllProperties()
    {
        var callOrder = new List<string>();
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => { callOrder.Add("cond1"); return None.Value; })
            .WithCondition(_ => { callOrder.Add("cond2"); return None.Value; })
            .WithModify(m => { callOrder.Add("modify"); m.Name = "enriched"; })
            .WithBeforeExecution(_ => callOrder.Add("before"))
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(_ => callOrder.Add("after"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        var model = new TestModel();

        await builder.ConditionAsync(model);
        await builder.CreateModifyFunc(model);
        await builder.BeforeExecutionFunc(model);
        await builder.AfterExecutionFunc(model);

        Assert.Equal(["cond1", "cond2", "modify", "before", "after"], callOrder);
        Assert.Equal("enriched", model.Name);
    }

    #endregion

    #region Create - Result Variant with Hooks

    [Fact]
    public async Task CreateOneResult_FullChain_WithHooks()
    {
        var callOrder = new List<string>();
        var flow = new CommandOneResultFlow<TestModel, string>();

        ((IStartOneCommandResult<TestModel, string>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => { callOrder.Add("cond"); return None.Value; })
            .WithModify(m => { callOrder.Add("modify"); m.Name = "Enriched"; })
            .WithBeforeExecution(_ => callOrder.Add("before"))
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(_ => callOrder.Add("after"))
            .WithResultIfSucceed(m => m.Name);

        var builder = (ICommandOneFlowBuilderResult<TestModel, string>)flow;
        var model = new TestModel();

        await builder.ConditionAsync(model);
        await builder.CreateModifyFunc(model);
        await builder.BeforeExecutionFunc(model);
        await builder.AfterExecutionFunc(model);
        var result = builder.ResultFunc(model);

        Assert.Equal(["cond", "modify", "before", "after"], callOrder);
        Assert.Equal("Enriched", result);
    }

    [Fact]
    public void CreateOneResult_WithAfterExecution_ShouldReturnISaveChangesOneSucceed()
    {
        var flow = new CommandOneResultFlow<TestModel, string>();

        ((IStartOneCommandResult<TestModel, string>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(_ => { })
            .WithResultIfSucceed(m => m.Id);

        var builder = (ICommandOneFlowBuilderResult<TestModel, string>)flow;
        Assert.NotNull(builder.AfterExecutionFunc);
        Assert.NotNull(builder.ResultFunc);
    }

    #endregion

    #region Backward Compatibility

    [Fact]
    public void BackwardCompat_CreateVoid_SingleConditionDirectToSaveChange()
    {
        var flow = new CommandOneVoidFlow<TestModel>();

        var builder = ((IStartOneCommandVoid<TestModel>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error("Save failed"));

        var result = (ICommandOneFlowBuilderVoid<TestModel>)builder;
        Assert.Equal(CommandTypeOne.Create, result.CommandTypeOne);
        Assert.Null(result.CreateModifyFunc);
        Assert.Null(result.BeforeExecutionFunc);
        Assert.Null(result.AfterExecutionFunc);
    }

    [Fact]
    public void BackwardCompat_CreateResult_SingleConditionDirectToSaveChange()
    {
        var flow = new CommandOneResultFlow<TestModel, string>();

        ((IStartOneCommandResult<TestModel, string>)flow)
            .CreateOne(new TestModel { Id = "1" })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithResultIfSucceed(m => m.Id);

        var builder = (ICommandOneFlowBuilderResult<TestModel, string>)flow;
        Assert.Null(builder.CreateModifyFunc);
        Assert.Null(builder.BeforeExecutionFunc);
        Assert.Null(builder.AfterExecutionFunc);
    }

    #endregion

    #region Interface Reflection Tests

    [Fact]
    public void ICreateOneAfterConditionVoid_ShouldDefine_AllMethods()
    {
        var methods = typeof(ICreateOneAfterConditionVoid<>).GetMethods().Select(m => m.Name).ToList();
        Assert.Contains("WithCondition", methods);
        Assert.Contains("WithModify", methods);
        Assert.Contains("WithBeforeExecution", methods);
        Assert.Contains("WithErrorIfSaveChange", methods);
    }

    [Fact]
    public void IAfterSaveChangeVoid_ShouldDefine_WithAfterExecution()
    {
        var methods = typeof(IAfterSaveChangeVoid<>).GetMethods()
            .Where(m => m.DeclaringType == typeof(IAfterSaveChangeVoid<>))
            .Select(m => m.Name).ToList();
        Assert.Contains("WithAfterExecution", methods);
    }

    [Fact]
    public void IAfterSaveChangeVoid_ShouldInherit_ICommandOneFlowBuilderVoid()
    {
        var interfaces = typeof(IAfterSaveChangeVoid<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandOneFlowBuilderVoid<>));
    }

    [Fact]
    public void ICommandOneFlowBuilderVoid_ShouldDefine_HookProperties()
    {
        var type = typeof(ICommandOneFlowBuilderVoid<>);
        Assert.NotNull(type.GetProperty("BeforeExecutionFunc"));
        Assert.NotNull(type.GetProperty("AfterExecutionFunc"));
    }

    #endregion
}
