namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IRemoveManySpecialActionResult<TModel, TResult> where TModel : class
{
    IRemoveManyConditionResult<TModel, TResult> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
}

public interface IRemoveManySpecialActionVoid<TModel> where TModel : class
{
    IRemoveManyConditionVoid<TModel>
        WithSpecialAction(Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
}