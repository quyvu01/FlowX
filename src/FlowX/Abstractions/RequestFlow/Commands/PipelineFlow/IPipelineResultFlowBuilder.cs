namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public interface IPipelineResultFlowBuilder<TResult> : IPipelineFlowBuilder
{
    Func<object, Task<TResult>> ResultFuncAsync { get; }
}
