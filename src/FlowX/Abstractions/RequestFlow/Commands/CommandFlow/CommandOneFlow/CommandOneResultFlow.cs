using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public class CommandOneResultFlow<TModel, TResult> :
    IStartOneCommandResult<TModel, TResult>,
    ICreateOneConditionResult<TModel, TResult>,
    ICreateOneAfterConditionResult<TModel, TResult>,
    IUpdateOneSpecialActionResult<TModel, TResult>,
    IRemoveOneSpecialActionResult<TModel, TResult>,
    IUpdateOneConditionResult<TModel, TResult>,
    IUpdateOneAfterConditionResult<TModel, TResult>,
    IRemoveOneConditionResult<TModel, TResult>,
    ICommandOneErrorDetailResult<TModel, TResult>,
    ISaveChangesOneErrorDetailResult<TModel, TResult>,
    IAfterSaveChangeResult<TModel, TResult>,
    ICommandOneFlowBuilderResult<TModel, TResult>
    where TModel : class
{
    public CommandTypeOne CommandTypeOne { get; private set; } = CommandTypeOne.Unknown;
    public Func<Task<TModel>> ModelCreateFunc { get; private set; }
    public Func<TModel, Task<OneOf<None, Error>>> ConditionAsync { get; private set; }
    public Expression<Func<TModel, bool>> CommandFilter { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; private set; }
    public Func<TModel, Task> UpdateOneFunc { get; private set; }
    public Func<TModel, Task> CreateModifyFunc { get; private set; }
    public Func<TModel, Task> BeforeExecutionFunc { get; private set; }
    public Func<TModel, Task> AfterExecutionFunc { get; private set; }
    public Error NullError { get; private set; }
    public Error SaveChangesError { get; private set; }
    public Func<TModel, TResult> ResultFunc { get; private set; }

    // === ISaveChangesOneSucceed ===

    public ICommandOneFlowBuilderResult<TModel, TResult> WithResultIfSucceed(Func<TModel, TResult> resultFunc)
    {
        ResultFunc = resultFunc;
        return this;
    }

    // === IStartOneCommandResult ===

    public ICreateOneConditionResult<TModel, TResult> CreateOne(Func<Task<TModel>> modelFunc)
    {
        CommandTypeOne = CommandTypeOne.Create;
        ModelCreateFunc = modelFunc;
        return this;
    }

    public ICreateOneConditionResult<TModel, TResult> CreateOne(Func<TModel> modelFunc)
    {
        CommandTypeOne = CommandTypeOne.Create;
        ModelCreateFunc = () => Task.FromResult(modelFunc.Invoke());
        return this;
    }

    public ICreateOneConditionResult<TModel, TResult> CreateOne(TModel model)
    {
        CommandTypeOne = CommandTypeOne.Create;
        ModelCreateFunc = () => Task.FromResult(model);
        return this;
    }

    public IUpdateOneSpecialActionResult<TModel, TResult> UpdateOne(Expression<Func<TModel, bool>> filter)
    {
        CommandTypeOne = CommandTypeOne.Update;
        CommandFilter = filter;
        return this;
    }

    public IRemoveOneSpecialActionResult<TModel, TResult> RemoveOne(Expression<Func<TModel, bool>> filter)
    {
        CommandTypeOne = CommandTypeOne.Remove;
        CommandFilter = filter;
        return this;
    }

    // === SpecialAction ===

    IUpdateOneConditionResult<TModel, TResult> IUpdateOneSpecialActionResult<TModel, TResult>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        CommandSpecialAction = specialAction;
        return this;
    }

    IRemoveOneConditionResult<TModel, TResult> IRemoveOneSpecialActionResult<TModel, TResult>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        CommandSpecialAction = specialAction;
        return this;
    }

    // === WithModify (public - satisfies IUpdateOneModifyResult, inherited by IUpdateOneAfterConditionResult) ===

    public ICommandOneErrorDetailResult<TModel, TResult> WithModify(Func<TModel, Task> updateFuncAsync)
    {
        UpdateOneFunc = updateFuncAsync;
        return this;
    }

    public ICommandOneErrorDetailResult<TModel, TResult> WithModify(Action<TModel> updateFunc)
    {
        UpdateOneFunc = model =>
        {
            updateFunc.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    // === WithErrorIfNull ===

    public ISaveChangesOneErrorDetailResult<TModel, TResult> WithErrorIfNull(Error error)
    {
        NullError = error;
        return this;
    }

    // === ISaveChangesOneErrorDetailResult ===

    IAfterSaveChangeResult<TModel, TResult> ISaveChangesOneErrorDetailResult<TModel, TResult>.WithErrorIfSaveChange(
        Error error)
    {
        SaveChangesError = error;
        return this;
    }

    ISaveChangesOneErrorDetailResult<TModel, TResult> ISaveChangesOneErrorDetailResult<TModel, TResult>
        .WithBeforeExecution(Action<TModel> action)
    {
        BeforeExecutionFunc = model =>
        {
            action.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ISaveChangesOneErrorDetailResult<TModel, TResult> ISaveChangesOneErrorDetailResult<TModel, TResult>
        .WithBeforeExecution(Func<TModel, Task> actionAsync)
    {
        BeforeExecutionFunc = actionAsync;
        return this;
    }

    // === IAfterSaveChangeResult ===

    ISaveChangesOneSucceed<TModel, TResult> IAfterSaveChangeResult<TModel, TResult>.WithAfterExecution(
        Action<TModel> action)
    {
        AfterExecutionFunc = model =>
        {
            action.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ISaveChangesOneSucceed<TModel, TResult> IAfterSaveChangeResult<TModel, TResult>.WithAfterExecution(
        Func<TModel, Task> actionAsync)
    {
        AfterExecutionFunc = actionAsync;
        return this;
    }

    // === ICreateOneConditionResult.WithCondition (first condition for Create) ===

    ICreateOneAfterConditionResult<TModel, TResult> ICreateOneConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ConditionAsync = model => Task.FromResult(condition(model));
        return this;
    }

    ICreateOneAfterConditionResult<TModel, TResult> ICreateOneConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    // === ICreateOneAfterConditionResult (chain conditions, modify, hooks for Create) ===

    ICreateOneAfterConditionResult<TModel, TResult> ICreateOneAfterConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        var prev = ConditionAsync;
        ConditionAsync = async model =>
        {
            var result = await prev(model);
            if (result.IsT1) return result;
            return condition(model);
        };
        return this;
    }

    ICreateOneAfterConditionResult<TModel, TResult> ICreateOneAfterConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        var prev = ConditionAsync;
        ConditionAsync = async model =>
        {
            var result = await prev(model);
            if (result.IsT1) return result;
            return await conditionAsync(model);
        };
        return this;
    }

    ISaveChangesOneErrorDetailResult<TModel, TResult> ICreateOneAfterConditionResult<TModel, TResult>.WithModify(
        Action<TModel> modifyAction)
    {
        CreateModifyFunc = model =>
        {
            modifyAction.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ISaveChangesOneErrorDetailResult<TModel, TResult> ICreateOneAfterConditionResult<TModel, TResult>.WithModify(
        Func<TModel, Task> modifyActionAsync)
    {
        CreateModifyFunc = modifyActionAsync;
        return this;
    }

    ISaveChangesOneErrorDetailResult<TModel, TResult> ICreateOneAfterConditionResult<TModel, TResult>
        .WithBeforeExecution(Action<TModel> action)
    {
        BeforeExecutionFunc = model =>
        {
            action.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ISaveChangesOneErrorDetailResult<TModel, TResult> ICreateOneAfterConditionResult<TModel, TResult>
        .WithBeforeExecution(Func<TModel, Task> actionAsync)
    {
        BeforeExecutionFunc = actionAsync;
        return this;
    }

    IAfterSaveChangeResult<TModel, TResult> ICreateOneAfterConditionResult<TModel, TResult>.WithErrorIfSaveChange(
        Error error)
    {
        SaveChangesError = error;
        return this;
    }

    // === IRemoveOneConditionResult.WithCondition ===

    ICommandOneErrorDetailResult<TModel, TResult> IRemoveOneConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    ICommandOneErrorDetailResult<TModel, TResult> IRemoveOneConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ConditionAsync = model => Task.FromResult(condition(model));
        return this;
    }

    // === IUpdateOneConditionResult.WithCondition (first condition for Update) ===

    IUpdateOneAfterConditionResult<TModel, TResult> IUpdateOneConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    IUpdateOneAfterConditionResult<TModel, TResult> IUpdateOneConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ConditionAsync = model => Task.FromResult(condition(model));
        return this;
    }

    // === IUpdateOneAfterConditionResult (chain conditions for Update) ===

    IUpdateOneAfterConditionResult<TModel, TResult> IUpdateOneAfterConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        var prev = ConditionAsync;
        ConditionAsync = async model =>
        {
            var result = await prev(model);
            if (result.IsT1) return result;
            return condition(model);
        };
        return this;
    }

    IUpdateOneAfterConditionResult<TModel, TResult> IUpdateOneAfterConditionResult<TModel, TResult>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        var prev = ConditionAsync;
        ConditionAsync = async model =>
        {
            var result = await prev(model);
            if (result.IsT1) return result;
            return await conditionAsync(model);
        };
        return this;
    }
}
