using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryCountingPipelineStep<TModel, TPrev> : IQueryPipelineStepEntry where TModel : class
{
    public Expression<Func<TModel, bool>> Filter { get; set; }
    public Func<TPrev, Expression<Func<TModel, bool>>> FilterFactory { get; set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialAction { get; set; }
    public Func<TPrev, Func<IQueryable<TModel>, IQueryable<TModel>>> SpecialActionFactory { get; set; }

    public async Task<object> ExecuteAsync(
        IQueryPipelineServiceProvider provider, object previousResult, CancellationToken ct)
    {
        var filter = FilterFactory is not null ? FilterFactory((TPrev)previousResult) : Filter;
        var specialAction = SpecialActionFactory is not null
            ? SpecialActionFactory((TPrev)previousResult)
            : SpecialAction;

        var count = await provider.GetCountAsync(filter, specialAction, ct);
        return count;
    }
}
