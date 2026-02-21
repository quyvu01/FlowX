namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public interface IPipelineContinuationOrTerminal<out TPrev> : IPipelineContinuation<TPrev>, IPipelineFlowBuilder;