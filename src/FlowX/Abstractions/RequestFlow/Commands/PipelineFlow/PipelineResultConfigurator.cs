using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

internal sealed class PipelineResultConfigurator<TResult> :
    IPipelineResultTerminal<TResult>
{
    private readonly CommandPipelineFlow _commandPipeline;

    internal PipelineResultConfigurator(CommandPipelineFlow commandPipeline)
    {
        _commandPipeline = commandPipeline;
    }

    // ===== IPipelineFlowBuilder =====

    IReadOnlyList<IPipelineStepEntry> IPipelineFlowBuilder.Steps => _commandPipeline.StepEntries;
    Error IPipelineFlowBuilder.SaveChangesError => _commandPipeline.SaveChangesErrorValue;
    Func<Task> IPipelineFlowBuilder.BeforeExecutionFunc => _commandPipeline.BeforeExecutionFuncValue;
    Func<Task> IPipelineFlowBuilder.AfterExecutionFunc => _commandPipeline.AfterExecutionFuncValue;

    // ===== IPipelineResultFlowBuilder<TResult> =====

    Func<object, Task<TResult>> IPipelineResultFlowBuilder<TResult>.ResultFuncAsync =>
        (Func<object, Task<TResult>>)_commandPipeline.ResultFuncAsyncValue;

    // ===== IPipelineResultTerminal<TResult> =====

    IPipelineResultTerminal<TResult> IPipelineResultTerminal<TResult>.WithErrorIfSaveChange(Error error)
    {
        _commandPipeline.SaveChangesErrorValue = error;
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineResultTerminal<TResult>.WithBeforeExecution(Action action)
    {
        _commandPipeline.BeforeExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineResultTerminal<TResult> IPipelineResultTerminal<TResult>.WithBeforeExecution(Func<Task> actionAsync)
    {
        _commandPipeline.BeforeExecutionFuncValue = actionAsync;
        return this;
    }

    IPipelineResultFlowBuilder<TResult> IPipelineResultTerminal<TResult>.WithAfterExecution(Action action)
    {
        _commandPipeline.AfterExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IPipelineResultFlowBuilder<TResult> IPipelineResultTerminal<TResult>.WithAfterExecution(Func<Task> actionAsync)
    {
        _commandPipeline.AfterExecutionFuncValue = actionAsync;
        return this;
    }
}
