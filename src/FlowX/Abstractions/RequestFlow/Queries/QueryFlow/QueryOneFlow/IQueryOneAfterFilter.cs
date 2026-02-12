using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneAfterFilter<TModel, TResponse> : IQueryOneSpecialAction<TModel, TResponse>
    where TModel : class where TResponse : class
{
    IQueryOneAfterFilter<TModel, TResponse> WithFilter(Expression<Func<TModel, bool>> filter);
}
