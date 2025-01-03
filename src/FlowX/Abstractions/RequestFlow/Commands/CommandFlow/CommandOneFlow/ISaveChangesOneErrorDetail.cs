using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ISaveChangesOneErrorDetailResult<TModel, TResult> where TModel : class
{
    ISaveChangesOneSucceed<TModel, TResult> WithErrorIfSaveChange([NotNull] Error error);
}

public interface ISaveChangesOneErrorDetailVoid<TModel> where TModel : class
{
    ICommandOneFlowBuilderVoid<TModel> WithErrorIfSaveChange([NotNull] Error error);
}