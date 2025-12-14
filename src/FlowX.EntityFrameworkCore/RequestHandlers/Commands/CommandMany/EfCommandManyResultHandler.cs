using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Extensions;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;

public abstract class EfCommandManyResultHandler<TModel, TCommand, TResult>
    : ICommandHandler<TCommand, TResult>
    where TModel : class
    where TCommand : class, ICommandResult<TResult>
{
    protected abstract ICommandManyFlowBuilderResult<TModel, TResult> BuildCommand(
        IStartManyCommandResult<TModel, TResult> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<TResult> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var repository = unitOfWork.RepositoryOf<TModel>();
        var buildResult = BuildCommand(new CommandManyResultFlow<TModel, TResult>(), requestContext);
        var commandType = buildResult.CommandTypeMany;
        TModel[] items;
        switch (commandType)
        {
            case CommandTypeMany.Create:
                var itemsCreating = (await buildResult.ModelsAsync.Invoke()).ToArray();
                if (buildResult.ConditionAsync is { } conditionCreatingAsync)
                {
                    var conditionResult = await conditionCreatingAsync.Invoke(itemsCreating);
                    conditionResult.ThrowIfError();
                }

                await repository.CreateManyAsync(itemsCreating, token: requestContext.CancellationToken);
                items = itemsCreating;
                break;
            case CommandTypeMany.Update:
                var itemsUpdating = (await repository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken)).ToArray();
                if (buildResult.ConditionAsync is { } conditionUpdatingAsync)
                {
                    var conditionResult = await conditionUpdatingAsync.Invoke(itemsUpdating);
                    conditionResult.ThrowIfError();
                }

                await buildResult.UpdateManyFunc.Invoke(itemsUpdating);
                items = itemsUpdating;
                break;
            case CommandTypeMany.Remove:
                var itemsRemoving = (await repository
                    .GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken)).ToArray();
                if (buildResult.ConditionAsync is { } conditionRemovingAsync)
                {
                    var conditionResult = await conditionRemovingAsync.Invoke(itemsRemoving);
                    conditionResult.ThrowIfError();
                }

                await repository.RemoveManyAsync(itemsRemoving, requestContext.CancellationToken);
                items = itemsRemoving;
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        await unitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        var result = buildResult.ResultFunc.Invoke(items);
        return result;
    }
}