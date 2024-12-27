using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListSortedField<TModel, out TResponse> where TModel : class
{
    IQueryListSortedDirection<TModel, TResponse> WithSortFieldWhenNotSet(Expression<Func<TModel, object>> expression);
}