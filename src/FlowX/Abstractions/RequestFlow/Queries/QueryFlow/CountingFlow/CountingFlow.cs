using System.Linq.Expressions;
using FlowX.Extensions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public class CountingFlowStart : ICountingFilter
{
    public ICountingAfterFilter<TModel> WithFilter<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
        => new CountingFlowConfigurator<TModel>(filter);
}

internal sealed class CountingFlowConfigurator<TModel> :
    ICountingAfterFilter<TModel>
    where TModel : class
{
    private Expression<Func<TModel, bool>> _filter;

    internal CountingFlowConfigurator(Expression<Func<TModel, bool>> filter)
    {
        _filter = filter;
    }

    // === ICountingAfterFilter (chain filters) ===

    ICountingAfterFilter<TModel> ICountingAfterFilter<TModel>.WithFilter(
        Expression<Func<TModel, bool>> filter)
    {
        _filter = _filter.AndAlso(filter);
        return this;
    }

    // === ICountingFlowBuilder — Execution ===

    public Task<long> ExecuteCountAsync(IQueryFlowServiceProvider provider, CancellationToken ct)
        => provider.LongCountAsync(_filter, null, ct);
}
