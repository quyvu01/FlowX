using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListAfterFilter<TModel, TResponse> : IQueryListSpecialAction<TModel, TResponse>
    where TModel : class
{
    IQueryListAfterFilter<TModel, TResponse> WithFilter(Expression<Func<TModel, bool>> filter);
}
