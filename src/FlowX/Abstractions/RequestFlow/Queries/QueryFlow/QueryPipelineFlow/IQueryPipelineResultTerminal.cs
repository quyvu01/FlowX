using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineResultTerminal<TResult> : IQueryPipelineFlowBuilder<TResult>
{
    IQueryPipelineResultTerminal<TResult> WithBeforeExecution([NotNull] Action action);
    IQueryPipelineResultTerminal<TResult> WithBeforeExecution([NotNull] Func<Task> actionAsync);
    IQueryPipelineFlowBuilder<TResult> WithAfterExecution([NotNull] Action action);
    IQueryPipelineFlowBuilder<TResult> WithAfterExecution([NotNull] Func<Task> actionAsync);
}
