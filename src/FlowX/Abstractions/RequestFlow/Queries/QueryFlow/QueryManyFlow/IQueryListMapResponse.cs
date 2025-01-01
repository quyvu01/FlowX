namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListMapResponse<TModel, TResponse> where TModel : class
{
    IQueryListSortedField<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc);
}