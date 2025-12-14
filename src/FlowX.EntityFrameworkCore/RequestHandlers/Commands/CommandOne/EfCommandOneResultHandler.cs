using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Extensions;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;

public abstract class EfCommandOneResultHandler<TModel, TCommand, TResult>
    : ICommandHandler<TCommand, TResult>
    where TModel : class
    where TCommand : class, ICommandResult<TResult>
{
    protected abstract ICommandOneFlowBuilderResult<TModel, TResult> BuildCommand(
        IStartOneCommandResult<TModel, TResult> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<TResult> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var repository = unitOfWork.RepositoryOf<TModel>();
        var buildResult = BuildCommand(new CommandOneResultFlow<TModel, TResult>(), requestContext);
        var commandType = buildResult.CommandTypeOne;
        TModel item;
        switch (commandType)
        {
            case CommandTypeOne.Create:
                var itemCreating = await buildResult.ModelCreateFunc.Invoke();
                if (buildResult.ConditionAsync is { } conditionCreatingAsync)
                {
                    var conditionResult = await conditionCreatingAsync.Invoke(itemCreating);
                    conditionResult.ThrowIfError();
                }

                await repository.CreateOneAsync(itemCreating, token: requestContext.CancellationToken);
                item = itemCreating;
                break;
            case CommandTypeOne.Update:
                var itemUpdating = await repository
                    .GetFirstByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                if (buildResult.ConditionAsync is { } conditionUpdatingAsync)
                {
                    var conditionResult = await conditionUpdatingAsync.Invoke(itemUpdating);
                    conditionResult.ThrowIfError();
                }

                await buildResult.UpdateOneFunc.Invoke(itemUpdating);
                item = itemUpdating;
                break;
            case CommandTypeOne.Remove:
                var itemRemoving = await repository.GetFirstByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                if (buildResult.ConditionAsync is { } conditionRemovingAsync)
                {
                    var conditionResult = await conditionRemovingAsync.Invoke(itemRemoving);
                    conditionResult.ThrowIfError();
                }

                await repository.RemoveOneAsync(itemRemoving, requestContext.CancellationToken);
                item = itemRemoving;
                break;
            case CommandTypeOne.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        await unitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        var result = buildResult.ResultFunc.Invoke(item);
        return result;
    }
}