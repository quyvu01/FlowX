using System.Linq.Expressions;
using FlowX.Extensions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public class CountingFlow<TModel> :
    ICountingFilter<TModel>,
    ICountingAfterFilter<TModel>,
    ICountingFlowBuilder<TModel> where TModel : class
{
    public Expression<Func<TModel, bool>> Filter { get; private set; }

    // === ICountingFilter (first filter) ===

    ICountingAfterFilter<TModel> ICountingFilter<TModel>.WithFilter(Expression<Func<TModel, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    // === ICountingAfterFilter (chain filters) ===

    ICountingAfterFilter<TModel> ICountingAfterFilter<TModel>.WithFilter(Expression<Func<TModel, bool>> filter)
    {
        Filter = Filter.AndAlso(filter);
        return this;
    }
}
