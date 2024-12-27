using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListFilter<TModel, TResponse> where TModel : class
{
    IQueryListSpecialAction<TModel, TResponse> WithFilter(Expression<Func<TModel, bool>> filter);
}