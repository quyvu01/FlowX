using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

internal sealed class PipelineManyStepConfigurator<TModel, TPrev> :
    IPipelineManyCreateStep<TModel>,
    IPipelineManyUpdateStep<TModel>,
    IPipelineManyRemoveStep<TModel>,
    IPipelineAfterDone<List<TModel>>,
    IPipelineContinuationOrTerminal<List<TModel>>,
    IPipelineTerminal
    where TModel : class
{
    private readonly CommandPipelineFlow _commandPipeline;
    private readonly IPipelineStepEntry _currentStep;

    internal PipelineManyStepConfigurator(CommandPipelineFlow commandPipeline, IPipelineStepEntry currentStep)
    {
        _commandPipeline = commandPipeline;
        _currentStep = currentStep;
    }

    // ===== IPipelineFlowBuilder =====

    IReadOnlyList<IPipelineStepEntry> IPipelineFlowBuilder.Steps => _commandPipeline.StepEntries;
    Error IPipelineFlowBuilder.SaveChangesError => _commandPipeline.SaveChangesErrorValue;
    Func<Task> IPipelineFlowBuilder.BeforeExecutionFunc => _commandPipeline.BeforeExecutionFuncValue;
    Func<Task> IPipelineFlowBuilder.AfterExecutionFunc => _commandPipeline.AfterExecutionFuncValue;

    // ===== IPipelineManyCreateStep<TModel> =====

    IPipelineManyCreateStep<TModel> IPipelineManyCreateStep<TModel>.WithCondition(
        Func<List<TModel>, OneOf<None, Error>> condition)
    {
        ComposeConditionOnCreate(models => Task.FromResult(condition(models)));
        return this;
    }

    IPipelineManyCreateStep<TModel> IPipelineManyCreateStep<TModel>.WithCondition(
        Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnCreate(conditionAsync);
        return this;
    }

    IPipelineManyCreateStep<TModel> IPipelineManyCreateStep<TModel>.WithModify(Action<TModel> modifyAction)
    {
        var step = (CreateManyPipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = models =>
        {
            foreach (var model in models) modifyAction(model);
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineManyCreateStep<TModel> IPipelineManyCreateStep<TModel>.WithModify(Func<TModel, Task> modifyActionAsync)
    {
        var step = (CreateManyPipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = async models =>
        {
            foreach (var model in models) await modifyActionAsync(model);
        };
        return this;
    }

    // ===== IPipelineManyUpdateStep<TModel> =====

    IPipelineManyUpdateStep<TModel> IPipelineManyUpdateStep<TModel>.WithCondition(
        Func<List<TModel>, OneOf<None, Error>> condition)
    {
        ComposeConditionOnUpdate(models => Task.FromResult(condition(models)));
        return this;
    }

    IPipelineManyUpdateStep<TModel> IPipelineManyUpdateStep<TModel>.WithCondition(
        Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnUpdate(conditionAsync);
        return this;
    }

    IPipelineManyUpdateStep<TModel> IPipelineManyUpdateStep<TModel>.WithModify(Action<List<TModel>> modifyAction)
    {
        var step = (UpdateManyPipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = models =>
        {
            modifyAction(models);
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineManyUpdateStep<TModel> IPipelineManyUpdateStep<TModel>.WithModify(Func<List<TModel>, Task> modifyActionAsync)
    {
        var step = (UpdateManyPipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = modifyActionAsync;
        return this;
    }

    // ===== IPipelineManyRemoveStep<TModel> =====

    IPipelineManyRemoveStep<TModel> IPipelineManyRemoveStep<TModel>.WithCondition(
        Func<List<TModel>, OneOf<None, Error>> condition)
    {
        ComposeConditionOnRemove(models => Task.FromResult(condition(models)));
        return this;
    }

    IPipelineManyRemoveStep<TModel> IPipelineManyRemoveStep<TModel>.WithCondition(
        Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnRemove(conditionAsync);
        return this;
    }

    // ===== IPipelineContinuation<List<TModel>> (shared transitions) =====

    IPipelineOneCreateStep<TNext> IPipelineContinuation<List<TModel>>.ThenCreateOne<TNext>(
        Func<List<TModel>, TNext> factory)
    {
        var step = new CreateOnePipelineStep<TNext, List<TModel>>
        {
            ModelFromPrevFunc = prev => Task.FromResult(factory(prev))
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    IPipelineOneCreateStep<TNext> IPipelineContinuation<List<TModel>>.ThenCreateOne<TNext>(
        Func<List<TModel>, Task<TNext>> factoryAsync)
    {
        var step = new CreateOnePipelineStep<TNext, List<TModel>>
        {
            ModelFromPrevFunc = factoryAsync
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    IPipelineOneUpdateStep<TNext> IPipelineContinuation<List<TModel>>.ThenUpdateOne<TNext>(
        Func<List<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new UpdateOnePipelineStep<TNext, List<TModel>>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    IPipelineOneRemoveStep<TNext> IPipelineContinuation<List<TModel>>.ThenRemoveOne<TNext>(
        Func<List<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new RemoveOnePipelineStep<TNext, List<TModel>>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    IPipelineManyCreateStep<TNext> IPipelineContinuation<List<TModel>>.ThenCreateMany<TNext>(
        Func<List<TModel>, IEnumerable<TNext>> factory)
    {
        var step = new CreateManyPipelineStep<TNext, List<TModel>>
        {
            ModelsFromPrevFunc = prev => Task.FromResult(factory(prev))
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    IPipelineManyCreateStep<TNext> IPipelineContinuation<List<TModel>>.ThenCreateMany<TNext>(
        Func<List<TModel>, Task<IEnumerable<TNext>>> factoryAsync)
    {
        var step = new CreateManyPipelineStep<TNext, List<TModel>>
        {
            ModelsFromPrevFunc = factoryAsync
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    IPipelineManyUpdateStep<TNext> IPipelineContinuation<List<TModel>>.ThenUpdateMany<TNext>(
        Func<List<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new UpdateManyPipelineStep<TNext, List<TModel>>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    IPipelineManyRemoveStep<TNext> IPipelineContinuation<List<TModel>>.ThenRemoveMany<TNext>(
        Func<List<TModel>, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new RemoveManyPipelineStep<TNext, List<TModel>>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, List<TModel>>(_commandPipeline, step);
    }

    // ===== IPipelineNextable<List<TModel>> =====

    IPipelineAfterDone<List<TModel>> IPipelineNextable<List<TModel>>.Done()
    {
        _currentStep.IsTransactionBoundary = true;
        return this;
    }

    IPipelineTerminal IPipelineNextable<List<TModel>>.WithErrorIfSaveChange(Error error)
    {
        _commandPipeline.SaveChangesErrorValue = error;
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineNextable<List<TModel>>.WithResultIfSucceed<TResult>(
        Func<List<TModel>, TResult> resultFunc)
        => ((IPipelineNextable<List<TModel>>)this).WithResultIfSucceed(
            models => Task.FromResult(resultFunc(models)));

    IPipelineResultTerminal<TResult> IPipelineNextable<List<TModel>>.WithResultIfSucceed<TResult>(
        Func<List<TModel>, Task<TResult>> resultFuncAsync)
    {
        _commandPipeline.ResultFuncAsyncValue =
            new Func<object, Task<TResult>>(obj => resultFuncAsync((List<TModel>)obj));
        return new PipelineResultConfigurator<TResult>(_commandPipeline);
    }

    // ===== IPipelineAfterDone<List<TModel>> =====

    IPipelineContinuationOrTerminal<List<TModel>> IPipelineAfterDone<List<TModel>>.WithErrorIfSaveChange(Error error)
    {
        _commandPipeline.SaveChangesErrorValue = error;
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineAfterDone<List<TModel>>.WithResultIfSucceed<TResult>(
        Func<List<TModel>, TResult> resultFunc)
        => ((IPipelineAfterDone<List<TModel>>)this).WithResultIfSucceed(
            models => Task.FromResult(resultFunc(models)));

    IPipelineResultTerminal<TResult> IPipelineAfterDone<List<TModel>>.WithResultIfSucceed<TResult>(
        Func<List<TModel>, Task<TResult>> resultFuncAsync)
    {
        _commandPipeline.ResultFuncAsyncValue =
            new Func<object, Task<TResult>>(obj => resultFuncAsync((List<TModel>)obj));
        return new PipelineResultConfigurator<TResult>(_commandPipeline);
    }

    // ===== IPipelineTerminal =====

    IPipelineTerminal IPipelineTerminal.WithBeforeExecution(Action action)
    {
        _commandPipeline.BeforeExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineTerminal IPipelineTerminal.WithBeforeExecution(Func<Task> actionAsync)
    {
        _commandPipeline.BeforeExecutionFuncValue = actionAsync;
        return this;
    }

    IPipelineFlowBuilder IPipelineTerminal.WithAfterExecution(Action action)
    {
        _commandPipeline.AfterExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineFlowBuilder IPipelineTerminal.WithAfterExecution(Func<Task> actionAsync)
    {
        _commandPipeline.AfterExecutionFuncValue = actionAsync;
        return this;
    }

    // ===== Private helpers for condition composition =====

    private void ComposeConditionOnCreate(Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        var step = (CreateManyPipelineStep<TModel, TPrev>)_currentStep;
        if (step.ConditionAsync is null)
        {
            step.ConditionAsync = conditionAsync;
        }
        else
        {
            var prev = step.ConditionAsync;
            step.ConditionAsync = async models =>
            {
                var result = await prev(models);
                if (result.IsT1) return result;
                return await conditionAsync(models);
            };
        }
    }

    private void ComposeConditionOnUpdate(Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        var step = (UpdateManyPipelineStep<TModel, TPrev>)_currentStep;
        if (step.ConditionAsync is null)
        {
            step.ConditionAsync = conditionAsync;
        }
        else
        {
            var prev = step.ConditionAsync;
            step.ConditionAsync = async models =>
            {
                var result = await prev(models);
                if (result.IsT1) return result;
                return await conditionAsync(models);
            };
        }
    }

    private void ComposeConditionOnRemove(Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync)
    {
        var step = (RemoveManyPipelineStep<TModel, TPrev>)_currentStep;
        if (step.ConditionAsync is null)
        {
            step.ConditionAsync = conditionAsync;
        }
        else
        {
            var prev = step.ConditionAsync;
            step.ConditionAsync = async models =>
            {
                var result = await prev(models);
                if (result.IsT1) return result;
                return await conditionAsync(models);
            };
        }
    }
}
