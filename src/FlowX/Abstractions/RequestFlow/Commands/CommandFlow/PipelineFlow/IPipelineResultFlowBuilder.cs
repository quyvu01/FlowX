namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineResultFlowBuilder<TResult> : IPipelineFlowBuilder
{
    Func<object, Task<TResult>> ResultFuncAsync { get; }
}
