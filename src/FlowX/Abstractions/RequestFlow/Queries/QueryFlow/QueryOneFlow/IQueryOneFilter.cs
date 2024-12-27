using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneFilter<TModel, TResponse> where TModel : class where TResponse : class
{
    IQueryOneSpecialAction<TModel, TResponse> WithFilter(Expression<Func<TModel, bool>> filter);
}