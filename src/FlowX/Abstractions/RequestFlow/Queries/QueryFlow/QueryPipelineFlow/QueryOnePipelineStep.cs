using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryOnePipelineStep<TModel, TPrev> : IQueryPipelineStepEntry where TModel : class
{
    // First step: direct filter. Subsequent: filter from prev
    public Expression<Func<TModel, bool>> Filter { get; set; }
    public Func<TPrev, Expression<Func<TModel, bool>>> FilterFactory { get; set; }

    // Static special action (WithSpecialAction on step)
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialAction { get; set; }

    // Dynamic special action from prev (FromQueryable variant)
    public Func<TPrev, Func<IQueryable<TModel>, IQueryable<TModel>>> SpecialActionFactory { get; set; }

    public Error NullError { get; set; }

    public async Task<object> ExecuteAsync(
        IQueryPipelineServiceProvider provider, object previousResult, CancellationToken ct)
    {
        var filter = FilterFactory is not null ? FilterFactory((TPrev)previousResult) : Filter;
        var specialAction = SpecialActionFactory is not null
            ? SpecialActionFactory((TPrev)previousResult)
            : SpecialAction;

        var result = await provider.GetFirstByConditionAsync(filter, specialAction, ct);
        if (result is null && NullError is not null) throw NullError;
        return result;
    }
}
