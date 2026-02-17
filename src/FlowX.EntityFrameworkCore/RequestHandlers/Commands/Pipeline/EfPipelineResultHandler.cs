using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;
using FlowX.EntityFrameworkCore.SharedStates;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.Pipeline;

public abstract class EfPipelineResultHandler<TCommand, TResult>
    : ICommandHandler<TCommand, TResult>
    where TCommand : class, ICommandResult<TResult>
{
    protected abstract IPipelineResultFlowBuilder<TResult> BuildPipeline(
        IStartPipeline fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<TResult> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var provider = new EfPipelineServiceProvider(unitOfWork);
        var builder = BuildPipeline(new PipelineFlow(), requestContext);

        if (builder.BeforeExecutionFunc is { } beforeFunc)
            await beforeFunc.Invoke();

        object previousResult = null;
        foreach (var step in builder.Steps)
        {
            previousResult = await step.ExecuteAsync(provider, previousResult, requestContext.CancellationToken);
            if (step.IsTransactionBoundary)
                await provider.SaveChangesAsync(requestContext.CancellationToken);
        }

        try
        {
            await provider.SaveChangesAsync(requestContext.CancellationToken);
        }
        catch when (builder.SaveChangesError is { } saveChangesError)
        {
            throw saveChangesError;
        }

        if (builder.AfterExecutionFunc is { } afterFunc)
            await afterFunc.Invoke();

        return await builder.ResultFuncAsync(previousResult);
    }
}
