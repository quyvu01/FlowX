using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Extensions;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

internal sealed class UpdateManyPipelineStep<TModel, TPrev> : IPipelineStepEntry
    where TModel : class
{
    public bool IsTransactionBoundary { get; set; }
    public Expression<Func<TModel, bool>> Filter { get; set; }
    public Func<TPrev, Expression<Func<TModel, bool>>> FilterFactory { get; set; }
    public Func<List<TModel>, Task<OneOf<None, Error>>> ConditionAsync { get; set; }
    public Func<List<TModel>, Task> ModifyFunc { get; set; }

    public async Task<object> ExecuteAsync(IPipelineServiceProvider provider, object previousResult,
        CancellationToken ct)
    {
        var filter = FilterFactory is not null
            ? FilterFactory((TPrev)previousResult)
            : Filter;

        var models = await provider.GetManyByConditionAsync(filter, ct);

        if (ConditionAsync is { } conditionAsync)
        {
            var result = await conditionAsync(models);
            result.ThrowIfError();
        }

        if (ModifyFunc is { } modifyFunc) await modifyFunc(models);

        return models;
    }
}