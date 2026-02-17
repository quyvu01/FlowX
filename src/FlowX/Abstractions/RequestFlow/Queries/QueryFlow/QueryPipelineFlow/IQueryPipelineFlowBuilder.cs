namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineFlowBuilder<TResult>
{
    IReadOnlyList<IQueryPipelineStepEntry> Steps { get; }
    Func<object, Task<TResult>> ResultFuncAsync { get; }
    Func<Task> BeforeExecutionFunc { get; }
    Func<Task> AfterExecutionFunc { get; }
}
