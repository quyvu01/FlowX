using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface ISaveChangesManyErrorDetailResult<TModel, TResult> where TModel : class
{
    ISaveChangesManySucceedResult<TModel, TResult> WithErrorIfSaveChange([NotNull] Error error);
}

public interface ISaveChangesManyErrorDetailVoid<TModel> where TModel : class
{
    ICommandManyFlowBuilderVoid<TModel> WithErrorIfSaveChange([NotNull] Error error);
}