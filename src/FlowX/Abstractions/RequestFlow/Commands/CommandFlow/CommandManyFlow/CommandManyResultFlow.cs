using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public class CommandManyResultFlow<TModel, TResult> :
    IStartManyCommandResult<TModel, TResult>,
    ICreateManyConditionResult<TModel, TResult>,
    IUpdateManySpecialActionResult<TModel, TResult>,
    IRemoveManySpecialActionResult<TModel, TResult>,
    IUpdateManyConditionResult<TModel, TResult>,
    IRemoveManyConditionResult<TModel, TResult>,
    IUpdateManyModifyResult<TModel, TResult>,
    ICommandManyErrorDetailResult<TModel, TResult>,
    ISaveChangesManyErrorDetailResult<TModel, TResult>,
    ISaveChangesManySucceedResult<TModel, TResult>,
    ICommandManyFlowBuilderResult<TModel, TResult>
    where TModel : class
{
    public ICreateManyConditionResult<TModel, TResult> CreateMany(Func<Task<List<TModel>>> modelsFunc)
    {
        CommandTypeMany = CommandTypeMany.Create;
        ModelsCreateFunc = modelsFunc;
        return this;
    }

    public ICreateManyConditionResult<TModel, TResult> CreateMany(Func<List<TModel>> modelsFunc)
    {
        CommandTypeMany = CommandTypeMany.Create;
        ModelsCreateFunc = () => Task.FromResult(modelsFunc.Invoke());
        return this;
    }

    public ICreateManyConditionResult<TModel, TResult> CreateMany(List<TModel> models)
    {
        CommandTypeMany = CommandTypeMany.Create;
        ModelsCreateFunc = () => Task.FromResult(models);
        return this;
    }

    public IUpdateManySpecialActionResult<TModel, TResult> UpdateMany(Expression<Func<TModel, bool>> filter)
    {
        CommandTypeMany = CommandTypeMany.Update;
        CommandFilter = filter;
        return this;
    }

    IUpdateManyConditionResult<TModel, TResult> IUpdateManySpecialActionResult<TModel, TResult>.
        WithSpecialAction(
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        CommandSpecialAction = specialAction;
        return this;
    }

    public ISaveChangesManyErrorDetailResult<TModel, TResult> WithModify(Func<List<TModel>, Task> updateFuncAsync)
    {
        UpdateManyFunc = updateFuncAsync;
        return this;
    }

    public ISaveChangesManyErrorDetailResult<TModel, TResult> WithModify(Action<List<TModel>> updateFunc)
    {
        UpdateManyFunc = models =>
        {
            updateFunc.Invoke(models);
            return Task.CompletedTask;
        };
        return this;
    }


    IRemoveManyConditionResult<TModel, TResult> IRemoveManySpecialActionResult<TModel, TResult>.
        WithSpecialAction(
            Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        CommandSpecialAction = specialAction;
        return this;
    }

    public IRemoveManySpecialActionResult<TModel, TResult> RemoveMany(Expression<Func<TModel, bool>> filter)
    {
        CommandTypeMany = CommandTypeMany.Remove;
        CommandFilter = filter;
        return this;
    }

    public ISaveChangesManySucceedResult<TModel, TResult> WithErrorIfSaveChange(Error error)
    {
        SaveChangesError = error;
        return this;
    }

    ISaveChangesManyErrorDetailResult<TModel, TResult> ICommandManyErrorDetailResult<TModel, TResult>.
        WithErrorIfNull(Error error)
    {
        NullError = error;
        return this;
    }

    public ICommandManyFlowBuilderResult<TModel, TResult> WithResultIfSucceed(
        Func<List<TModel>, TResult> resultFunc)
    {
        ResultFunc = resultFunc;
        return this;
    }

    public CommandTypeMany CommandTypeMany { get; private set; } = CommandTypeMany.Unknown;
    public Func<Task<List<TModel>>> ModelsCreateFunc { get; private set; }
    public Func<IReadOnlyCollection<TModel>, Task<OneOf<None, Error>>> ConditionAsync { get; private set; }
    public Expression<Func<TModel, bool>> CommandFilter { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; private set; }
    public Func<List<TModel>, Task> UpdateManyFunc { get; private set; }
    public Error NullError { get; private set; }
    public Error SaveChangesError { get; private set; }
    public Func<List<TModel>, TResult> ResultFunc { get; private set; }

    ISaveChangesManyErrorDetailResult<TModel, TResult> ICreateManyConditionResult<TModel, TResult>.WithCondition(
        Func<IReadOnlyCollection<TModel>, OneOf<None, Error>> condition)
    {
        ConditionAsync = models => Task.FromResult(condition(models));
        return this;
    }

    ISaveChangesManyErrorDetailResult<TModel, TResult> IRemoveManyConditionResult<TModel, TResult>.WithCondition(
        Func<IReadOnlyCollection<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    ISaveChangesManyErrorDetailResult<TModel, TResult> IRemoveManyConditionResult<TModel, TResult>.WithCondition(
        Func<IReadOnlyCollection<TModel>, OneOf<None, Error>> condition)
    {
        ConditionAsync = models => Task.FromResult(condition(models));
        return this;
    }

    IUpdateManyModifyResult<TModel, TResult> IUpdateManyConditionResult<TModel, TResult>.WithCondition(
        Func<IReadOnlyCollection<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }

    IUpdateManyModifyResult<TModel, TResult> IUpdateManyConditionResult<TModel, TResult>.WithCondition(
        Func<IReadOnlyCollection<TModel>, OneOf<None, Error>> condition)
    {
        ConditionAsync = models => Task.FromResult(condition(models));
        return this;
    }

    ISaveChangesManyErrorDetailResult<TModel, TResult> ICreateManyConditionResult<TModel, TResult>.WithCondition(
        Func<IReadOnlyCollection<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        ConditionAsync = conditionAsync;
        return this;
    }
}