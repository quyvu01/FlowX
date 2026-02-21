using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public interface IPipelineFlowBuilder
{
    IReadOnlyList<IPipelineStepEntry> Steps { get; }
    Error SaveChangesError { get; }
    Func<Task> BeforeExecutionFunc { get; }
    Func<Task> AfterExecutionFunc { get; }
}
