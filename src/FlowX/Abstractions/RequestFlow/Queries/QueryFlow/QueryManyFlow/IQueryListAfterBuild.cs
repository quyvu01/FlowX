using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListAfterBuild<TModel, TResponse> : IQueryListFlowBuilder<TModel, TResponse>
    where TModel : class
{
    IQueryListFlowBuilder<TModel, TResponse> WithAfterExecution([NotNull] Action action);
    IQueryListFlowBuilder<TModel, TResponse> WithAfterExecution([NotNull] Func<Task> actionAsync);
}
