using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneErrorDetail<TModel, out TResponse> where TModel : class where TResponse : class
{
    IQueryOneFlowBuilder<TModel, TResponse> WithErrorIfNull([NotNull] Error error);
}