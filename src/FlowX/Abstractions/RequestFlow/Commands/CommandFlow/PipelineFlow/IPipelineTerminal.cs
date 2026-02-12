using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineTerminal : IPipelineFlowBuilder
{
    IPipelineTerminal WithBeforeExecution([NotNull] Action action);
    IPipelineTerminal WithBeforeExecution([NotNull] Func<Task> actionAsync);
    IPipelineFlowBuilder WithAfterExecution([NotNull] Action action);
    IPipelineFlowBuilder WithAfterExecution([NotNull] Func<Task> actionAsync);
}
