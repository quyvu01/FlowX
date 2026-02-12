using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

internal sealed class PipelineStepConfigurator<TModel, TPrev> :
    IPipelineCreateStep<TModel>,
    IPipelineUpdateStep<TModel>,
    IPipelineRemoveStep<TModel>,
    IPipelineNextable<TModel>,
    IPipelineAfterDone<TModel>,
    IPipelineTerminal,
    IPipelineFlowBuilder
    where TModel : class
{
    private readonly PipelineFlow _pipeline;
    private readonly IPipelineStepEntry _currentStep;

    internal PipelineStepConfigurator(PipelineFlow pipeline, IPipelineStepEntry currentStep)
    {
        _pipeline = pipeline;
        _currentStep = currentStep;
    }

    // ===== IPipelineFlowBuilder =====

    IReadOnlyList<IPipelineStepEntry> IPipelineFlowBuilder.Steps => _pipeline.StepEntries;
    Error IPipelineFlowBuilder.SaveChangesError => _pipeline.SaveChangesErrorValue;
    Func<Task> IPipelineFlowBuilder.BeforeExecutionFunc => _pipeline.BeforeExecutionFuncValue;
    Func<Task> IPipelineFlowBuilder.AfterExecutionFunc => _pipeline.AfterExecutionFuncValue;

    // ===== IPipelineCreateStep<TModel> =====

    IPipelineCreateStep<TModel> IPipelineCreateStep<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ComposeConditionOnCreate(model => Task.FromResult(condition(model)));
        return this;
    }

    IPipelineCreateStep<TModel> IPipelineCreateStep<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnCreate(conditionAsync);
        return this;
    }

    IPipelineCreateStep<TModel> IPipelineCreateStep<TModel>.WithModify(Action<TModel> modifyAction)
    {
        var step = (CreatePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = model =>
        {
            modifyAction(model);
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineCreateStep<TModel> IPipelineCreateStep<TModel>.WithModify(Func<TModel, Task> modifyActionAsync)
    {
        var step = (CreatePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = modifyActionAsync;
        return this;
    }

    // ===== IPipelineUpdateStep<TModel> =====

    IPipelineUpdateStep<TModel> IPipelineUpdateStep<TModel>.WithErrorIfNull(Error error)
    {
        var step = (UpdatePipelineStep<TModel, TPrev>)_currentStep;
        step.NullError = error;
        return this;
    }

    IPipelineUpdateStep<TModel> IPipelineUpdateStep<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ComposeConditionOnUpdate(model => Task.FromResult(condition(model)));
        return this;
    }

    IPipelineUpdateStep<TModel> IPipelineUpdateStep<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnUpdate(conditionAsync);
        return this;
    }

    IPipelineUpdateStep<TModel> IPipelineUpdateStep<TModel>.WithModify(Action<TModel> modifyAction)
    {
        var step = (UpdatePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = model =>
        {
            modifyAction(model);
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineUpdateStep<TModel> IPipelineUpdateStep<TModel>.WithModify(Func<TModel, Task> modifyActionAsync)
    {
        var step = (UpdatePipelineStep<TModel, TPrev>)_currentStep;
        step.ModifyFunc = modifyActionAsync;
        return this;
    }

    // ===== IPipelineRemoveStep<TModel> =====

    IPipelineRemoveStep<TModel> IPipelineRemoveStep<TModel>.WithErrorIfNull(Error error)
    {
        var step = (RemovePipelineStep<TModel, TPrev>)_currentStep;
        step.NullError = error;
        return this;
    }

    IPipelineRemoveStep<TModel> IPipelineRemoveStep<TModel>.WithCondition(
        Func<TModel, OneOf<None, Error>> condition)
    {
        ComposeConditionOnRemove(model => Task.FromResult(condition(model)));
        return this;
    }

    IPipelineRemoveStep<TModel> IPipelineRemoveStep<TModel>.WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        ComposeConditionOnRemove(conditionAsync);
        return this;
    }

    // ===== IPipelineNextable<TModel> =====

    IPipelineAfterDone<TModel> IPipelineNextable<TModel>.Done()
    {
        _currentStep.IsTransactionBoundary = true;
        return this;
    }

    IPipelineCreateStep<TNext> IPipelineNextable<TModel>.ThenCreateOne<TNext>(Func<TModel, TNext> factory)
    {
        var step = new CreatePipelineStep<TNext, TModel>
        {
            ModelFromPrevFunc = prev => Task.FromResult(factory(prev))
        };
        _pipeline.StepEntries.Add(step);
        return new PipelineStepConfigurator<TNext, TModel>(_pipeline, step);
    }

    IPipelineCreateStep<TNext> IPipelineNextable<TModel>.ThenCreateOne<TNext>(
        Func<TModel, Task<TNext>> factoryAsync)
    {
        var step = new CreatePipelineStep<TNext, TModel>
        {
            ModelFromPrevFunc = factoryAsync
        };
        _pipeline.StepEntries.Add(step);
        return new PipelineStepConfigurator<TNext, TModel>(_pipeline, step);
    }

    IPipelineUpdateStep<TNext> IPipelineNextable<TModel>.ThenUpdateOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new UpdatePipelineStep<TNext, TModel>
        {
            FilterFactory = filterFactory
        };
        _pipeline.StepEntries.Add(step);
        return new PipelineStepConfigurator<TNext, TModel>(_pipeline, step);
    }

    IPipelineRemoveStep<TNext> IPipelineNextable<TModel>.ThenRemoveOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
    {
        var step = new RemovePipelineStep<TNext, TModel>
        {
            FilterFactory = filterFactory
        };
        _pipeline.StepEntries.Add(step);
        return new PipelineStepConfigurator<TNext, TModel>(_pipeline, step);
    }

    IPipelineTerminal IPipelineNextable<TModel>.WithErrorIfSaveChange(Error error)
    {
        _pipeline.SaveChangesErrorValue = error;
        return this;
    }

    // ===== IPipelineAfterDone<TModel> =====

    IPipelineCreateStep<TNext> IPipelineAfterDone<TModel>.ThenCreateOne<TNext>(Func<TModel, TNext> factory)
        => ((IPipelineNextable<TModel>)this).ThenCreateOne(factory);

    IPipelineCreateStep<TNext> IPipelineAfterDone<TModel>.ThenCreateOne<TNext>(
        Func<TModel, Task<TNext>> factoryAsync)
        => ((IPipelineNextable<TModel>)this).ThenCreateOne(factoryAsync);

    IPipelineUpdateStep<TNext> IPipelineAfterDone<TModel>.ThenUpdateOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
        => ((IPipelineNextable<TModel>)this).ThenUpdateOne(filterFactory);

    IPipelineRemoveStep<TNext> IPipelineAfterDone<TModel>.ThenRemoveOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory)
        => ((IPipelineNextable<TModel>)this).ThenRemoveOne(filterFactory);

    IPipelineTerminal IPipelineAfterDone<TModel>.WithErrorIfSaveChange(Error error)
        => ((IPipelineNextable<TModel>)this).WithErrorIfSaveChange(error);

    // ===== IPipelineTerminal =====

    IPipelineTerminal IPipelineTerminal.WithBeforeExecution(Action action)
    {
        _pipeline.BeforeExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineTerminal IPipelineTerminal.WithBeforeExecution(Func<Task> actionAsync)
    {
        _pipeline.BeforeExecutionFuncValue = actionAsync;
        return this;
    }

    IPipelineFlowBuilder IPipelineTerminal.WithAfterExecution(Action action)
    {
        _pipeline.AfterExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineFlowBuilder IPipelineTerminal.WithAfterExecution(Func<Task> actionAsync)
    {
        _pipeline.AfterExecutionFuncValue = actionAsync;
        return this;
    }

    // ===== Private helpers for condition composition =====

    private void ComposeConditionOnCreate(Func<TModel, Task<OneOf<None, Error>>> conditionAsync)
    {
        var step = (CreatePipelineStep<TModel, TPrev>)_currentStep;
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
        var step = (UpdatePipelineStep<TModel, TPrev>)_currentStep;
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
        var step = (RemovePipelineStep<TModel, TPrev>)_currentStep;
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
