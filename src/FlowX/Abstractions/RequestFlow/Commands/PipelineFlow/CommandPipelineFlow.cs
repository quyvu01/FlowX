using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public class CommandPipelineFlow : IStartCommandPipeline
{
    internal readonly List<IPipelineStepEntry> StepEntries = [];
    internal Error SaveChangesErrorValue;
    internal Func<Task> BeforeExecutionFuncValue;
    internal Func<Task> AfterExecutionFuncValue;
    internal object ResultFuncAsyncValue;

    // === IStartPipeline ===

    public IPipelineOneCreateStep<TModel> CreateOne<TModel>(TModel model) where TModel : class
        => CreateOne(() => Task.FromResult(model));

    public IPipelineOneCreateStep<TModel> CreateOne<TModel>(Func<TModel> modelFunc) where TModel : class
        => CreateOne(() => Task.FromResult(modelFunc()));

    public IPipelineOneCreateStep<TModel> CreateOne<TModel>(Func<Task<TModel>> modelFuncAsync) where TModel : class
    {
        var step = new CreateOnePipelineStep<TModel, object> { ModelFunc = modelFuncAsync };
        StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TModel, object>(this, step);
    }

    public IPipelineOneUpdateStep<TModel> UpdateOne<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
    {
        var step = new UpdateOnePipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TModel, object>(this, step);
    }

    public IPipelineOneRemoveStep<TModel> RemoveOne<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
    {
        var step = new RemoveOnePipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TModel, object>(this, step);
    }

    public IPipelineManyCreateStep<TModel> CreateMany<TModel>(IEnumerable<TModel> models) where TModel : class =>
        CreateMany(() => Task.FromResult(models));

    public IPipelineManyCreateStep<TModel> CreateMany<TModel>(Func<IEnumerable<TModel>> modelsFunc)
        where TModel : class => CreateMany(() => Task.FromResult(modelsFunc()));

    public IPipelineManyCreateStep<TModel> CreateMany<TModel>(Func<Task<IEnumerable<TModel>>> modelsFuncAsync)
        where TModel : class
    {
        var step = new CreateManyPipelineStep<TModel, object> { ModelsFunc = modelsFuncAsync };
        StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TModel, object>(this, step);
    }

    public IPipelineManyUpdateStep<TModel> UpdateMany<TModel>(Expression<Func<TModel, bool>> filter)
        where TModel : class
    {
        var step = new UpdateManyPipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TModel, object>(this, step);
    }

    public IPipelineManyRemoveStep<TModel> RemoveMany<TModel>(Expression<Func<TModel, bool>> filter)
        where TModel : class
    {
        var step = new RemoveManyPipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TModel, object>(this, step);
    }
}