using FlowX.Errors;
using FlowX.Extensions;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

internal sealed class CreateOnePipelineStep<TModel, TPrev> : IPipelineStepEntry
    where TModel : class
{
    public bool IsTransactionBoundary { get; set; }
    public Func<Task<TModel>> ModelFunc { get; set; }
    public Func<TPrev, Task<TModel>> ModelFromPrevFunc { get; set; }
    public Func<TModel, Task<OneOf<None, Error>>> ConditionAsync { get; set; }
    public Func<TModel, Task> ModifyFunc { get; set; }

    public async Task<object> ExecuteAsync(IPipelineServiceProvider provider, object previousResult,
        CancellationToken ct)
    {
        var model = ModelFromPrevFunc is not null
            ? await ModelFromPrevFunc((TPrev)previousResult)
            : await ModelFunc();

        if (ConditionAsync is { } conditionAsync)
        {
            var result = await conditionAsync(model);
            result.ThrowIfError();
        }

        if (ModifyFunc is { } modifyFunc)
            await modifyFunc(model);

        await provider.CreateOneAsync(model, ct);
        return model;
    }
}