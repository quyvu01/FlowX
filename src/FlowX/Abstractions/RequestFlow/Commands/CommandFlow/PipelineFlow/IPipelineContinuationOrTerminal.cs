namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineContinuationOrTerminal<out TPrev> : IPipelineContinuation<TPrev>, IPipelineFlowBuilder;