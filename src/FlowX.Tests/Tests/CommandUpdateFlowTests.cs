using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.Errors;
using FlowX.Structs;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class CommandUpdateFlowTests
{
    private sealed class TestModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    #region Update - Chainable Conditions

    [Fact]
    public async Task UpdateOne_WithMultipleConditions_ShouldComposeInOrder()
    {
        var callOrder = new List<int>();
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => { callOrder.Add(1); return None.Value; })
            .WithCondition(_ => { callOrder.Add(2); return None.Value; })
            .WithCondition(_ => { callOrder.Add(3); return None.Value; })
            .WithModify(m => m.Name = "updated")
            .WithErrorIfNull(new Error("Not found"))
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        var result = await builder.ConditionAsync(new TestModel());

        Assert.True(result.IsT0);
        Assert.Equal([1, 2, 3], callOrder);
        Assert.Equal(CommandTypeOne.Update, builder.CommandTypeOne);
    }

    [Fact]
    public async Task UpdateOne_WithMultipleConditions_ShouldFailFast()
    {
        var callOrder = new List<int>();
        var error = new Error("Validation failed");
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => { callOrder.Add(1); return None.Value; })
            .WithCondition(_ => { callOrder.Add(2); return error; })
            .WithCondition(_ => { callOrder.Add(3); return None.Value; })
            .WithModify(m => m.Name = "updated")
            .WithErrorIfNull(new Error("Not found"))
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        var result = await builder.ConditionAsync(new TestModel());

        Assert.True(result.IsT1);
        Assert.Same(error, result.AsT1);
        Assert.Equal([1, 2], callOrder);
    }

    [Fact]
    public async Task UpdateOne_WithSingleCondition_ShouldStillWork()
    {
        var called = false;
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => { called = true; return None.Value; })
            .WithModify(m => m.Name = "updated")
            .WithErrorIfNull(new Error("Not found"))
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        await builder.ConditionAsync(new TestModel());
        Assert.True(called);
    }

    #endregion

    #region Update - Before/After Hooks

    [Fact]
    public void UpdateOne_WithBeforeExecution_ShouldSet()
    {
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => None.Value)
            .WithModify(m => m.Name = "updated")
            .WithErrorIfNull(new Error("Not found"))
            .WithBeforeExecution(_ => { })
            .WithErrorIfSaveChange(new Error("Save failed"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        Assert.NotNull(builder.BeforeExecutionFunc);
    }

    [Fact]
    public void UpdateOne_WithAfterExecution_ShouldSet()
    {
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => None.Value)
            .WithModify(m => m.Name = "updated")
            .WithErrorIfNull(new Error("Not found"))
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(_ => { });

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        Assert.NotNull(builder.AfterExecutionFunc);
    }

    [Fact]
    public async Task UpdateOne_FullChain_ShouldBuildAllProperties()
    {
        var callOrder = new List<string>();
        var flow = new CommandOneVoidFlow<TestModel>();

        ((IStartOneCommandVoid<TestModel>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => { callOrder.Add("cond1"); return None.Value; })
            .WithCondition(_ => { callOrder.Add("cond2"); return None.Value; })
            .WithModify(m => { callOrder.Add("modify"); m.Name = "updated"; })
            .WithErrorIfNull(new Error("Not found"))
            .WithBeforeExecution(_ => callOrder.Add("before"))
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(_ => callOrder.Add("after"));

        var builder = (ICommandOneFlowBuilderVoid<TestModel>)flow;
        var model = new TestModel();

        await builder.ConditionAsync(model);
        await builder.UpdateOneFunc(model);
        await builder.BeforeExecutionFunc(model);
        await builder.AfterExecutionFunc(model);

        Assert.Equal(["cond1", "cond2", "modify", "before", "after"], callOrder);
        Assert.Equal("updated", model.Name);
    }

    #endregion

    #region Update - Result Variant

    [Fact]
    public async Task UpdateOneResult_WithMultipleConditions_ShouldCompose()
    {
        var callOrder = new List<int>();
        var flow = new CommandOneResultFlow<TestModel, string>();

        ((IStartOneCommandResult<TestModel, string>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => { callOrder.Add(1); return None.Value; })
            .WithCondition(_ => { callOrder.Add(2); return None.Value; })
            .WithModify(m => m.Name = "updated")
            .WithErrorIfNull(new Error("Not found"))
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithResultIfSucceed(m => m.Name);

        var builder = (ICommandOneFlowBuilderResult<TestModel, string>)flow;
        var result = await builder.ConditionAsync(new TestModel());

        Assert.True(result.IsT0);
        Assert.Equal([1, 2], callOrder);
    }

    [Fact]
    public async Task UpdateOneResult_FullChain_WithHooks()
    {
        var callOrder = new List<string>();
        var flow = new CommandOneResultFlow<TestModel, string>();

        ((IStartOneCommandResult<TestModel, string>)flow)
            .UpdateOne(x => x.Id == "1")
            .WithSpecialAction(q => q)
            .WithCondition(_ => { callOrder.Add("cond1"); return None.Value; })
            .WithCondition(_ => { callOrder.Add("cond2"); return None.Value; })
            .WithModify(m => { callOrder.Add("modify"); m.Name = "updated"; })
            .WithErrorIfNull(new Error("Not found"))
            .WithBeforeExecution(_ => callOrder.Add("before"))
            .WithErrorIfSaveChange(new Error("Save failed"))
            .WithAfterExecution(_ => callOrder.Add("after"))
            .WithResultIfSucceed(m => m.Name);

        var builder = (ICommandOneFlowBuilderResult<TestModel, string>)flow;
        var model = new TestModel();

        await builder.ConditionAsync(model);
        await builder.UpdateOneFunc(model);
        await builder.BeforeExecutionFunc(model);
        await builder.AfterExecutionFunc(model);
        var result = builder.ResultFunc(model);

        Assert.Equal(["cond1", "cond2", "modify", "before", "after"], callOrder);
        Assert.Equal("updated", result);
    }

    #endregion

    #region Update - Interface Reflection Tests

    [Fact]
    public void IUpdateOneAfterConditionVoid_ShouldExist()
    {
        Assert.True(typeof(IUpdateOneAfterConditionVoid<>).IsInterface);
    }

    [Fact]
    public void IUpdateOneAfterConditionVoid_ShouldInherit_IUpdateOneModifyVoid()
    {
        var interfaces = typeof(IUpdateOneAfterConditionVoid<>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUpdateOneModifyVoid<>));
    }

    [Fact]
    public void IUpdateOneAfterConditionVoid_ShouldDefine_WithCondition()
    {
        var methods = typeof(IUpdateOneAfterConditionVoid<>).GetMethods()
            .Where(m => m.DeclaringType == typeof(IUpdateOneAfterConditionVoid<>))
            .Select(m => m.Name).ToList();
        Assert.Contains("WithCondition", methods);
    }

    [Fact]
    public void IUpdateOneConditionVoid_ShouldReturn_IUpdateOneAfterConditionVoid()
    {
        var methods = typeof(IUpdateOneConditionVoid<>).GetMethods()
            .Where(m => m.Name == "WithCondition")
            .ToList();

        Assert.All(methods, m =>
            Assert.True(m.ReturnType.GetGenericTypeDefinition() == typeof(IUpdateOneAfterConditionVoid<>)));
    }

    [Fact]
    public void ISaveChangesOneErrorDetailVoid_ShouldDefine_WithBeforeExecution()
    {
        var methods = typeof(ISaveChangesOneErrorDetailVoid<>).GetMethods()
            .Select(m => m.Name).ToList();
        Assert.Contains("WithBeforeExecution", methods);
    }

    #endregion
}
