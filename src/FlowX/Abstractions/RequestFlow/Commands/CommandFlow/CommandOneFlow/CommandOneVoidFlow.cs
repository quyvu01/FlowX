using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public class CommandOneVoidFlow<TModel> :
    IStartOneCommandVoid<TModel>,
    ICreateOneConditionVoid<TModel>,
    ICreateOneAfterConditionVoid<TModel>,
    IUpdateOneSpecialActionVoid<TModel>,
    IRemoveOneSpecialActionVoid<TModel>,
    IUpdateOneConditionVoid<TModel>,
    IUpdateOneAfterConditionVoid<TModel>,
    IRemoveOneConditionVoid<TModel>,
    IUpdateOneModifyVoid<TModel>,
    ICommandOneErrorDetailVoid<TModel>,
    ISaveChangesOneErrorDetailVoid<TModel>,
    IAfterSaveChangeVoid<TModel>,
    ICommandOneFlowBuilderVoid<TModel>
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

    // === IStartOneCommandVoid ===

    public ICreateOneConditionVoid<TModel> CreateOne(Func<Task<TModel>> modelFunc)
    {
        CommandTypeOne = CommandTypeOne.Create;
        ModelCreateFunc = modelFunc;
        return this;
    }

    public ICreateOneConditionVoid<TModel> CreateOne(Func<TModel> modelFunc)
    {
        CommandTypeOne = CommandTypeOne.Create;
        ModelCreateFunc = () => Task.FromResult(modelFunc.Invoke());
        return this;
    }

    public ICreateOneConditionVoid<TModel> CreateOne(TModel model)
    {
        CommandTypeOne = CommandTypeOne.Create;
        ModelCreateFunc = () => Task.FromResult(model);
        return this;
    }

    public IUpdateOneSpecialActionVoid<TModel> UpdateOne(Expression<Func<TModel, bool>> filter)
    {
        CommandTypeOne = CommandTypeOne.Update;
        CommandFilter = filter;
        return this;
    }

    public IRemoveOneSpecialActionVoid<TModel> RemoveOne(Expression<Func<TModel, bool>> filter)
    {
        CommandTypeOne = CommandTypeOne.Remove;
        CommandFilter = filter;
        return this;
    }

    // === SpecialAction ===

    IUpdateOneConditionVoid<TModel> IUpdateOneSpecialActionVoid<TModel>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        CommandSpecialAction = specialAction;
        return this;
    }

    IRemoveOneConditionVoid<TModel> IRemoveOneSpecialActionVoid<TModel>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        CommandSpecialAction = specialAction;
        return this;
    }

    // === WithModify (public - satisfies IUpdateOneModifyVoid, inherited by IUpdateOneAfterConditionVoid) ===

    public ICommandOneErrorDetailVoid<TModel> WithModify(Func<TModel, Task> updateFuncAsync)
    {
        UpdateOneFunc = updateFuncAsync;
        return this;
    }

    public ICommandOneErrorDetailVoid<TModel> WithModify(Action<TModel> updateFunc)
    {
        UpdateOneFunc = model =>
        {
            updateFunc.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    // === WithErrorIfNull ===

    public ISaveChangesOneErrorDetailVoid<TModel> WithErrorIfNull(Error error)
    {
        NullError = error;
        return this;
    }

    // === ISaveChangesOneErrorDetailVoid ===

    IAfterSaveChangeVoid<TModel> ISaveChangesOneErrorDetailVoid<TModel>.WithErrorIfSaveChange(Error error)
    {
        SaveChangesError = error;
        return this;
    }

    ISaveChangesOneErrorDetailVoid<TModel> ISaveChangesOneErrorDetailVoid<TModel>.WithBeforeExecution(
        Action<TModel> action)
    {
        BeforeExecutionFunc = model =>
        {
            action.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ISaveChangesOneErrorDetailVoid<TModel> ISaveChangesOneErrorDetailVoid<TModel>.WithBeforeExecution(
        Func<TModel, Task> actionAsync)
    {
        BeforeExecutionFunc = actionAsync;
        return this;
    }

    // === IAfterSaveChangeVoid ===

    ICommandOneFlowBuilderVoid<TModel> IAfterSaveChangeVoid<TModel>.WithAfterExecution(Action<TModel> action)
    {
        AfterExecutionFunc = model =>
        {
            action.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ICommandOneFlowBuilderVoid<TModel> IAfterSaveChangeVoid<TModel>.WithAfterExecution(
        Func<TModel, Task> actionAsync)
    {
        AfterExecutionFunc = actionAsync;
        return this;
    }

    // === ICreateOneConditionVoid.WithCondition (first condition for Create) ===

    ICreateOneAfterConditionVoid<TModel> ICreateOneConditionVoid<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ConditionAsync = model => Task.FromResult(condition(model));
        return this;
    }

    ICreateOneAfterConditionVoid<TModel> ICreateOneConditionVoid<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    // === ICreateOneAfterConditionVoid (chain conditions, modify, hooks for Create) ===

    ICreateOneAfterConditionVoid<TModel> ICreateOneAfterConditionVoid<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        var prev = ConditionAsync;
        ConditionAsync = async model =>
        {
            var result = await prev(model);
            return result.IsT1 ? result : condition(model);
        };
        return this;
    }

    ICreateOneAfterConditionVoid<TModel> ICreateOneAfterConditionVoid<TModel>.WithCondition(
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

    ISaveChangesOneErrorDetailVoid<TModel> ICreateOneAfterConditionVoid<TModel>.WithModify(
        Action<TModel> modifyAction)
    {
        CreateModifyFunc = model =>
        {
            modifyAction.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ISaveChangesOneErrorDetailVoid<TModel> ICreateOneAfterConditionVoid<TModel>.WithModify(
        Func<TModel, Task> modifyActionAsync)
    {
        CreateModifyFunc = modifyActionAsync;
        return this;
    }

    ISaveChangesOneErrorDetailVoid<TModel> ICreateOneAfterConditionVoid<TModel>.WithBeforeExecution(
        Action<TModel> action)
    {
        BeforeExecutionFunc = model =>
        {
            action.Invoke(model);
            return Task.CompletedTask;
        };
        return this;
    }

    ISaveChangesOneErrorDetailVoid<TModel> ICreateOneAfterConditionVoid<TModel>.WithBeforeExecution(
        Func<TModel, Task> actionAsync)
    {
        BeforeExecutionFunc = actionAsync;
        return this;
    }

    IAfterSaveChangeVoid<TModel> ICreateOneAfterConditionVoid<TModel>.WithErrorIfSaveChange(Error error)
    {
        SaveChangesError = error;
        return this;
    }

    // === IRemoveOneConditionVoid.WithCondition ===

    ICommandOneErrorDetailVoid<TModel> IRemoveOneConditionVoid<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    ICommandOneErrorDetailVoid<TModel> IRemoveOneConditionVoid<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ConditionAsync = model => Task.FromResult(condition(model));
        return this;
    }

    // === IUpdateOneConditionVoid.WithCondition (first condition for Update) ===

    IUpdateOneAfterConditionVoid<TModel> IUpdateOneConditionVoid<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    IUpdateOneAfterConditionVoid<TModel> IUpdateOneConditionVoid<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ConditionAsync = model => Task.FromResult(condition(model));
        return this;
    }

    // === IUpdateOneAfterConditionVoid (chain conditions for Update) ===

    IUpdateOneAfterConditionVoid<TModel> IUpdateOneAfterConditionVoid<TModel>.WithCondition(
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

    IUpdateOneAfterConditionVoid<TModel> IUpdateOneAfterConditionVoid<TModel>.WithCondition(
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
