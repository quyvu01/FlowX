using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineResultTerminal<TResult> : IPipelineResultFlowBuilder<TResult>
{
    IPipelineResultTerminal<TResult> WithErrorIfSaveChange([NotNull] Error error);
    IPipelineResultTerminal<TResult> WithBeforeExecution([NotNull] Action action);
    IPipelineResultTerminal<TResult> WithBeforeExecution([NotNull] Func<Task> actionAsync);
    IPipelineResultFlowBuilder<TResult> WithAfterExecution([NotNull] Action action);
    IPipelineResultFlowBuilder<TResult> WithAfterExecution([NotNull] Func<Task> actionAsync);
}
