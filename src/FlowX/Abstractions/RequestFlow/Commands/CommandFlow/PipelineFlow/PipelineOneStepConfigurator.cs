using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

internal sealed class PipelineOneStepConfigurator<TModel, TPrev> :
    IPipelineOneCreateStep<TModel>,
    IPipelineOneUpdateStep<TModel>,
    IPipelineOneRemoveStep<TModel>,
    IPipelineAfterDone<TModel>,
    IPipelineContinuationOrTerminal<TModel>,
    IPipelineTerminal
    where TModel : class
{
    private readonly CommandPipelineFlow _commandPipeline;
    private readonly IPipelineStepEntry _currentStep;

    internal PipelineOneStepConfigurator(CommandPipelineFlow commandPipeline, IPipelineStepEntry currentStep)
    {
        _commandPipeline = commandPipeline;
        _currentStep = currentStep;
    }

    // ===== IPipelineFlowBuilder =====

    IReadOnlyList<IPipelineStepEntry> IPipelineFlowBuilder.Steps => _commandPipeline.StepEntries;
    Error IPipelineFlowBuilder.SaveChangesError => _commandPipeline.SaveChangesErrorValue;
    Func<Task> IPipelineFlowBuilder.BeforeExecutionFunc => _commandPipeline.BeforeExecutionFuncValue;
    Func<Task> IPipelineFlowBuilder.AfterExecutionFunc => _commandPipeline.AfterExecutionFuncValue;

    // ===== IPipelineOneCreateStep<TModel> =====

    IPipelineOneCreateStep<TModel> IPipelineOneCreateStep<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ComposeConditionOnCreate(model => Task.FromResult(condition(model)));
        return this;
    }

    IPipelineOneCreateStep<TModel> IPipelineOneCreateStep<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnCreate(conditionAsync);
        return this;
    }

    IPipelineOneCreateStep<TModel> IPipelineOneCreateStep<TModel>.WithModify(Action<TModel> modifyAction)
    {
        var step = (CreateOnePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = model =>
        {
            modifyAction(model);
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineOneCreateStep<TModel> IPipelineOneCreateStep<TModel>.WithModify(Func<TModel, Task> modifyActionAsync)
    {
        var step = (CreateOnePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = modifyActionAsync;
        return this;
    }

    // ===== IPipelineOneUpdateStep<TModel> =====

    IPipelineOneUpdateStep<TModel> IPipelineOneUpdateStep<TModel>.WithErrorIfNull(Error error)
    {
        var step = (UpdateOnePipelineStep<TModel, TPrev>)_currentStep;
        step.NullError = error;
        return this;
    }

    IPipelineOneUpdateStep<TModel> IPipelineOneUpdateStep<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ComposeConditionOnUpdate(model => Task.FromResult(condition(model)));
        return this;
    }

    IPipelineOneUpdateStep<TModel> IPipelineOneUpdateStep<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnUpdate(conditionAsync);
        return this;
    }

    IPipelineOneUpdateStep<TModel> IPipelineOneUpdateStep<TModel>.WithModify(Action<TModel> modifyAction)
    {
        var step = (UpdateOnePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = model =>
        {
            modifyAction(model);
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineOneUpdateStep<TModel> IPipelineOneUpdateStep<TModel>.WithModify(Func<TModel, Task> modifyActionAsync)
    {
        var step = (UpdateOnePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = modifyActionAsync;
        return this;
    }

    // ===== IPipelineOneRemoveStep<TModel> =====

    IPipelineOneRemoveStep<TModel> IPipelineOneRemoveStep<TModel>.WithErrorIfNull(Error error)
    {
        var step = (RemoveOnePipelineStep<TModel, TPrev>)_currentStep;
        step.NullError = error;
        return this;
    }

    IPipelineOneRemoveStep<TModel> IPipelineOneRemoveStep<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ComposeConditionOnRemove(model => Task.FromResult(condition(model)));
        return this;
    }

    IPipelineOneRemoveStep<TModel> IPipelineOneRemoveStep<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnRemove(conditionAsync);
        return this;
    }

    // ===== IPipelineContinuation<TModel> (shared by IPipelineNextable + IPipelineAfterDone path) =====

    IPipelineOneCreateStep<TNext> IPipelineContinuation<TModel>.ThenCreateOne<TNext>(Func<TModel, TNext> factory)
    {
        var step = new CreateOnePipelineStep<TNext, TModel>
        {
            ModelFromPrevFunc = prev => Task.FromResult(factory(prev))
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    IPipelineOneCreateStep<TNext> IPipelineContinuation<TModel>.ThenCreateOne<TNext>(
        Func<TModel, Task<TNext>> factoryAsync)
    {
        var step = new CreateOnePipelineStep<TNext, TModel>
        {
            ModelFromPrevFunc = factoryAsync
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    IPipelineOneUpdateStep<TNext> IPipelineContinuation<TModel>.ThenUpdateOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new UpdateOnePipelineStep<TNext, TModel>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    IPipelineOneRemoveStep<TNext> IPipelineContinuation<TModel>.ThenRemoveOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new RemoveOnePipelineStep<TNext, TModel>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineOneStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    IPipelineManyCreateStep<TNext> IPipelineContinuation<TModel>.ThenCreateMany<TNext>(
        Func<TModel, IEnumerable<TNext>> factory)
    {
        var step = new CreateManyPipelineStep<TNext, TModel>
        {
            ModelsFromPrevFunc = prev => Task.FromResult(factory(prev))
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    IPipelineManyCreateStep<TNext> IPipelineContinuation<TModel>.ThenCreateMany<TNext>(
        Func<TModel, Task<IEnumerable<TNext>>> factoryAsync)
    {
        var step = new CreateManyPipelineStep<TNext, TModel>
        {
            ModelsFromPrevFunc = factoryAsync
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    IPipelineManyUpdateStep<TNext> IPipelineContinuation<TModel>.ThenUpdateMany<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new UpdateManyPipelineStep<TNext, TModel>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    IPipelineManyRemoveStep<TNext> IPipelineContinuation<TModel>.ThenRemoveMany<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new RemoveManyPipelineStep<TNext, TModel>
        {
            FilterFactory = filterFactory
        };
        _commandPipeline.StepEntries.Add(step);
        return new PipelineManyStepConfigurator<TNext, TModel>(_commandPipeline, step);
    }

    // ===== IPipelineNextable<TModel> =====

    IPipelineAfterDone<TModel> IPipelineNextable<TModel>.Done()
    {
        _currentStep.IsTransactionBoundary = true;
        return this;
    }

    IPipelineTerminal IPipelineNextable<TModel>.WithErrorIfSaveChange(Error error)
    {
        _commandPipeline.SaveChangesErrorValue = error;
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineNextable<TModel>.WithResultIfSucceed<TResult>(
        Func<TModel, TResult> resultFunc)
        => ((IPipelineNextable<TModel>)this).WithResultIfSucceed(model => Task.FromResult(resultFunc(model)));

    IPipelineResultTerminal<TResult> IPipelineNextable<TModel>.WithResultIfSucceed<TResult>(
        Func<TModel, Task<TResult>> resultFuncAsync)
    {
        _commandPipeline.ResultFuncAsyncValue = new Func<object, Task<TResult>>(obj => resultFuncAsync((TModel)obj));
        return new PipelineResultConfigurator<TResult>(_commandPipeline);
    }

    // ===== IPipelineAfterDone<TModel> =====

    IPipelineContinuationOrTerminal<TModel> IPipelineAfterDone<TModel>.WithErrorIfSaveChange(Error error)
    {
        _commandPipeline.SaveChangesErrorValue = error;
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineAfterDone<TModel>.WithResultIfSucceed<TResult>(
        Func<TModel, TResult> resultFunc)
        => ((IPipelineAfterDone<TModel>)this).WithResultIfSucceed(model => Task.FromResult(resultFunc(model)));

    IPipelineResultTerminal<TResult> IPipelineAfterDone<TModel>.WithResultIfSucceed<TResult>(
        Func<TModel, Task<TResult>> resultFuncAsync)
    {
        _commandPipeline.ResultFuncAsyncValue = new Func<object, Task<TResult>>(obj => resultFuncAsync((TModel)obj));
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

    private void ComposeConditionOnCreate(Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        var step = (CreateOnePipelineStep<TModel, TPrev>)_currentStep;
        if (step.ConditionAsync is null)
        {
            step.ConditionAsync = conditionAsync;
        }
        else
        {
            var prev = step.ConditionAsync;
            step.ConditionAsync = async model =>
            {
                var result = await prev(model);
                if (result.IsT1) return result;
                return await conditionAsync(model);
            };
        }
    }

    private void ComposeConditionOnUpdate(Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        var step = (UpdateOnePipelineStep<TModel, TPrev>)_currentStep;
        if (step.ConditionAsync is null)
        {
            step.ConditionAsync = conditionAsync;
        }
        else
        {
            var prev = step.ConditionAsync;
            step.ConditionAsync = async model =>
            {
                var result = await prev(model);
                if (result.IsT1) return result;
                return await conditionAsync(model);
            };
        }
    }

    private void ComposeConditionOnRemove(Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        var step = (RemoveOnePipelineStep<TModel, TPrev>)_currentStep;
        if (step.ConditionAsync is null)
        {
            step.ConditionAsync = conditionAsync;
        }
        else
        {
            var prev = step.ConditionAsync;
            step.ConditionAsync = async model =>
            {
                var result = await prev(model);
                if (result.IsT1) return result;
                return await conditionAsync(model);
            };
        }
    }
}
