namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IUpdateManySpecialActionResult<TModel, TResult> where TModel : class
{
    IUpdateManyConditionResult<TModel, TResult> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
}

public interface IUpdateManySpecialActionVoid<TModel> where TModel : class
{
    IUpdateManyConditionVoid<TModel> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
}