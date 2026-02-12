using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Extensions;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

internal sealed class RemovePipelineStep<TModel, TPrev> : IPipelineStepEntry
    where TModel : class
{
    public bool IsTransactionBoundary { get; set; }
    public Expression<Func<TModel, bool>> Filter { get; set; }
    public Func<TPrev, Expression<Func<TModel, bool>>> FilterFactory { get; set; }
    public Error NullError { get; set; }
    public Func<TModel, Task<OneOf<None, Error>>> ConditionAsync { get; set; }

    public async Task<object> ExecuteAsync(IPipelineServiceProvider provider, object previousResult,
        CancellationToken ct)
    {
        var filter = FilterFactory is not null
            ? FilterFactory((TPrev)previousResult)
            : Filter;

        var model = await provider.GetFirstByConditionAsync(filter, ct);
        if (model is null) throw NullError ?? (Exception)new NullReferenceException($"Entity {typeof(TModel).Name} not found");

        if (ConditionAsync is { } conditionAsync)
        {
            var result = await conditionAsync(model);
            result.ThrowIfError();
        }

        await provider.RemoveOneAsync(model, ct);
        return model;
    }
}
