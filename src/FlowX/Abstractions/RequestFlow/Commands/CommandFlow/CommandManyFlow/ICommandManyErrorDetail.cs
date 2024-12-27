using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface ICommandManyErrorDetailResult<TModel, TResult> where TModel : class
{
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithErrorIfNull([NotNull] ErrorDetail errorDetail);
}
public interface ICommandManyErrorDetailVoid<TModel> where TModel : class
{
    ISaveChangesManyErrorDetailVoid<TModel> WithErrorIfNull([NotNull] ErrorDetail errorDetail);
}