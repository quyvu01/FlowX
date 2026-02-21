using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public interface IPipelineNextable<out TPrev> : IPipelineContinuation<TPrev>
{
    IPipelineAfterDone<TPrev> Done();
    IPipelineTerminal WithErrorIfSaveChange([NotNull] Error error);

    IPipelineResultTerminal<TResult> WithResultIfSucceed<TResult>(
        [NotNull] Func<TPrev, TResult> resultFunc);

    IPipelineResultTerminal<TResult> WithResultIfSucceed<TResult>(
        [NotNull] Func<TPrev, Task<TResult>> resultFuncAsync);
}
