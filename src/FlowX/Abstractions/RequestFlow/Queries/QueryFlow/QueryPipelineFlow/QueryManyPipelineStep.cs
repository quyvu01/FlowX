using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryManyPipelineStep<TModel, TPrev> : IQueryPipelineStepEntry where TModel : class
{
    // First step: direct filter. Subsequent: filter from prev
    public Expression<Func<TModel, bool>> Filter { get; set; }
    public Func<TPrev, Expression<Func<TModel, bool>>> FilterFactory { get; set; }

    // Static special action (WithSpecialAction on step)
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialAction { get; set; }

    // Dynamic special action from prev (FromQueryable variant)
    public Func<TPrev, Func<IQueryable<TModel>, IQueryable<TModel>>> SpecialActionFactory { get; set; }

    public async Task<object> ExecuteAsync(
        IQueryPipelineServiceProvider provider, object previousResult, CancellationToken ct)
    {
        var filter = FilterFactory is not null ? FilterFactory((TPrev)previousResult) : Filter;
        var specialAction = SpecialActionFactory is not null
            ? SpecialActionFactory((TPrev)previousResult)
            : SpecialAction;

        var result = await provider.GetManyByConditionAsync(filter, specialAction, ct);
        return result;
    }
}
