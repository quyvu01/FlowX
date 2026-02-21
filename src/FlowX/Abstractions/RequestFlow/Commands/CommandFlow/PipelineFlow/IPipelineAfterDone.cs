using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineAfterDone<out TPrev>
{
    IPipelineContinuationOrTerminal<TPrev> WithErrorIfSaveChange([NotNull] Error error);

    IPipelineResultTerminal<TResult> WithResultIfSucceed<TResult>([NotNull] Func<TPrev, TResult> resultFunc);

    IPipelineResultTerminal<TResult> WithResultIfSucceed<TResult>([NotNull] Func<TPrev, Task<TResult>> resultFuncAsync);
}
