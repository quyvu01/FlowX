using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListAfterBuild<TResponse> : IQueryListFlowBuilder<TResponse>
{
    IQueryListFlowBuilder<TResponse> WithAfterExecution([NotNull] Action action);
    IQueryListFlowBuilder<TResponse> WithAfterExecution([NotNull] Func<Task> actionAsync);
}
