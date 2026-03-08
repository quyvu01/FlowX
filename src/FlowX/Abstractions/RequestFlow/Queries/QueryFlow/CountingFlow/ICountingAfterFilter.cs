using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public interface ICountingAfterFilter<TModel> : ICountingFlowBuilder
    where TModel : class
{
    ICountingAfterFilter<TModel> WithFilter(Expression<Func<TModel, bool>> filter);
}
