using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListSortedField<TModel, TResponse> where TModel : class
{
    IQueryListAfterBuild<TResponse> WithDefaultSortFields(ExpressionOrder<TModel> expressionOrder);
    IQueryListSortedField<TModel, TResponse> WithBeforeExecution([NotNull] Action action);
    IQueryListSortedField<TModel, TResponse> WithBeforeExecution([NotNull] Func<Task> actionAsync);
}
