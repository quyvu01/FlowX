using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneAfterBuild<TResponse> : IQueryOneFlowBuilder<TResponse>
    where TResponse : class
{
    IQueryOneFlowBuilder<TResponse> WithAfterExecution([NotNull] Action<TResponse> action);
    IQueryOneFlowBuilder<TResponse> WithAfterExecution([NotNull] Func<TResponse, Task> actionAsync);
}
