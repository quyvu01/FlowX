using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICommandOneErrorDetailResult<TModel, TResult> where TModel : class
{
    ISaveChangesOneErrorDetailResult<TModel, TResult> WithErrorIfNull([NotNull] Error error);
}

public interface ICommandOneErrorDetailVoid<TModel> where TModel : class
{
    ISaveChangesOneErrorDetailVoid<TModel> WithErrorIfNull([NotNull] Error error);
}