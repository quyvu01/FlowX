namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IRemoveOneSpecialActionResult<TModel, TResult> where TModel : class
{
    IRemoveOneConditionResult<TModel, TResult>
        WithSpecialAction(Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
}

public interface IRemoveOneSpecialActionVoid<TModel> where TModel : class
{
    IRemoveOneConditionVoid<TModel>
        WithSpecialAction(Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
}