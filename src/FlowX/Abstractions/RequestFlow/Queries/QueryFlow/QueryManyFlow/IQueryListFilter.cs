using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListFilter<TResponse>
{
    IQueryListAfterFilter<TModel, TResponse> WithFilter<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class;
}
