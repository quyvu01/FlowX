using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineFlowBuilder
{
    IReadOnlyList<IPipelineStepEntry> Steps { get; }
    Error SaveChangesError { get; }
    Func<Task> BeforeExecutionFunc { get; }
    Func<Task> AfterExecutionFunc { get; }
}
