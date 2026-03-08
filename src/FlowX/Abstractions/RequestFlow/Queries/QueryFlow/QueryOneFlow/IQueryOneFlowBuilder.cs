using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneFlowBuilder<TResponse> where TResponse : class
{
    Error NullError { get; }
    Func<Task> BeforeExecutionFunc { get; }
    Func<TResponse, Task> AfterExecutionFunc { get; }

    Task<TResponse> ExecuteOneAsync(
        IQueryFlowServiceProvider provider,
        CancellationToken ct);
}
