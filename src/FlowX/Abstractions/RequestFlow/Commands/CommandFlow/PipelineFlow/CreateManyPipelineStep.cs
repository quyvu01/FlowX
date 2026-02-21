using FlowX.Errors;
using FlowX.Extensions;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

internal sealed class CreateManyPipelineStep<TModel, TPrev> : IPipelineStepEntry
    where TModel : class
{
    public bool IsTransactionBoundary { get; set; }
    public Func<Task<IEnumerable<TModel>>> ModelsFunc { get; set; }
    public Func<TPrev, Task<IEnumerable<TModel>>> ModelsFromPrevFunc { get; set; }
    public Func<List<TModel>, Task<OneOf<None, Error>>> ConditionAsync { get; set; }
    public Func<List<TModel>, Task> ModifyFunc { get; set; }

    public async Task<object> ExecuteAsync(IPipelineServiceProvider provider, object previousResult,
        CancellationToken ct)
    {
        var models = ModelsFromPrevFunc is not null
            ? await ModelsFromPrevFunc((TPrev)previousResult)
            : await ModelsFunc();

        var modelsAsList = models.ToList();

        if (ConditionAsync is { } conditionAsync)
        {
            var result = await conditionAsync(modelsAsList);
            result.ThrowIfError();
        }

        if (ModifyFunc is { } modifyFunc) await modifyFunc(modelsAsList);

        await provider.CreateManyAsync(modelsAsList, ct);
        return modelsAsList;
    }
}