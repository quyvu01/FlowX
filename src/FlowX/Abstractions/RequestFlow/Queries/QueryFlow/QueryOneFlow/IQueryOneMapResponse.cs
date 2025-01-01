namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneMapResponse<TModel, TResponse> where TModel : class where TResponse : class
{
    IQueryOneErrorDetail<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc);
}