using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public interface ICountingFilter<TModel> where TModel : class
{
    ICountingFlowBuilder<TModel> WithFilter(Expression<Func<TModel, bool>> filter);
}