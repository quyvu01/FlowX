namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneSpecialAction<TModel, TResponse> where TModel : class where TResponse : class
{
    IQueryOneMapResponse<TModel, TResponse> WithSpecialAction(Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);
    IQueryOneErrorDetail<TModel, TResponse> WithSpecialAction(Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction);
}