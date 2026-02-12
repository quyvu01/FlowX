using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneAfterBuild<TModel, TResponse> : IQueryOneFlowBuilder<TModel, TResponse>
    where TModel : class where TResponse : class
{
    IQueryOneFlowBuilder<TModel, TResponse> WithAfterExecution([NotNull] Action<TResponse> action);
    IQueryOneFlowBuilder<TModel, TResponse> WithAfterExecution([NotNull] Func<TResponse, Task> actionAsync);
}
