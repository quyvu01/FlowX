using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public class PipelineFlow : IStartPipeline
{
    internal readonly List<IPipelineStepEntry> StepEntries = [];
    internal Error SaveChangesErrorValue;
    internal Func<Task> BeforeExecutionFuncValue;
    internal Func<Task> AfterExecutionFuncValue;

    // === IStartPipeline ===

    public IPipelineCreateStep<TModel> CreateOne<TModel>(TModel model) where TModel : class
        => CreateOne(() => Task.FromResult(model));

    public IPipelineCreateStep<TModel> CreateOne<TModel>(Func<TModel> modelFunc) where TModel : class
        => CreateOne(() => Task.FromResult(modelFunc()));

    public IPipelineCreateStep<TModel> CreateOne<TModel>(Func<Task<TModel>> modelFuncAsync) where TModel : class
    {
        var step = new CreatePipelineStep<TModel, object> { ModelFunc = modelFuncAsync };
        StepEntries.Add(step);
        return new PipelineStepConfigurator<TModel, object>(this, step);
    }

    public IPipelineUpdateStep<TModel> UpdateOne<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
    {
        var step = new UpdatePipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new PipelineStepConfigurator<TModel, object>(this, step);
    }

    public IPipelineRemoveStep<TModel> RemoveOne<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
    {
        var step = new RemovePipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new PipelineStepConfigurator<TModel, object>(this, step);
    }
}
