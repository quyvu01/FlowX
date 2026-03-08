using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneFilter<TResponse> where TResponse : class
{
    IQueryOneAfterFilter<TModel, TResponse> WithFilter<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class;
}
