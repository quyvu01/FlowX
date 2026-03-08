using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneErrorDetail<TModel, TResponse> where TModel : class where TResponse : class
{
    IQueryOneAfterBuild<TResponse> WithErrorIfNull([NotNull] Error error);
    IQueryOneErrorDetail<TModel, TResponse> WithBeforeExecution([NotNull] Action action);
    IQueryOneErrorDetail<TModel, TResponse> WithBeforeExecution([NotNull] Func<Task> actionAsync);
}
