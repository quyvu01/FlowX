using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

internal sealed class PipelineResultConfigurator<TResult> :
    IPipelineResultTerminal<TResult>
{
    private readonly PipelineFlow _pipeline;

    internal PipelineResultConfigurator(PipelineFlow pipeline)
    {
        _pipeline = pipeline;
    }

    // ===== IPipelineFlowBuilder =====

    IReadOnlyList<IPipelineStepEntry> IPipelineFlowBuilder.Steps => _pipeline.StepEntries;
    Error IPipelineFlowBuilder.SaveChangesError => _pipeline.SaveChangesErrorValue;
    Func<Task> IPipelineFlowBuilder.BeforeExecutionFunc => _pipeline.BeforeExecutionFuncValue;
    Func<Task> IPipelineFlowBuilder.AfterExecutionFunc => _pipeline.AfterExecutionFuncValue;

    // ===== IPipelineResultFlowBuilder<TResult> =====

    Func<object, Task<TResult>> IPipelineResultFlowBuilder<TResult>.ResultFuncAsync =>
        (Func<object, Task<TResult>>)_pipeline.ResultFuncAsyncValue;

    // ===== IPipelineResultTerminal<TResult> =====

    IPipelineResultTerminal<TResult> IPipelineResultTerminal<TResult>.WithErrorIfSaveChange(Error error)
    {
        _pipeline.SaveChangesErrorValue = error;
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineResultTerminal<TResult>.WithBeforeExecution(Action action)
    {
        _pipeline.BeforeExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineResultTerminal<TResult>.WithBeforeExecution(Func<Task> actionAsync)
    {
        _pipeline.BeforeExecutionFuncValue = actionAsync;
        return this;
    }

    IPipelineResultFlowBuilder<TResult> IPipelineResultTerminal<TResult>.WithAfterExecution(Action action)
    {
        _pipeline.AfterExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineResultFlowBuilder<TResult> IPipelineResultTerminal<TResult>.WithAfterExecution(Func<Task> actionAsync)
    {
        _pipeline.AfterExecutionFuncValue = actionAsync;
        return this;
    }
}
