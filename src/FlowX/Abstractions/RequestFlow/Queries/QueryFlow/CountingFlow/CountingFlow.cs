using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public class CountingFlow<TModel> :
    ICountingFilter<TModel>,
    ICountingFlowBuilder<TModel> where TModel : class
{
    public ICountingFlowBuilder<TModel> WithFilter(Expression<Func<TModel, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    public Expression<Func<TModel, bool>> Filter { get; private set; }
}