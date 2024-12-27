namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListSpecialAction<TModel, TResponse> where TModel : class
{
    IQueryListSortedField<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);

    IQueryListSortedField<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction);
}