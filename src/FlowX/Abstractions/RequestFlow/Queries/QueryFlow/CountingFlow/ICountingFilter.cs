using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public interface ICountingFilter
{
    ICountingAfterFilter<TModel> WithFilter<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class;
}
