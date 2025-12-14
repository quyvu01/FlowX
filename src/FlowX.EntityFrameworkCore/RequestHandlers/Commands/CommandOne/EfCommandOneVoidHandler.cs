using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Extensions;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;

public abstract class EfCommandOneVoidHandler<TModel, TCommand>
    : ICommandHandler<TCommand, None>
    where TModel : class
    where TCommand : class, ICommandVoid
{
    protected abstract ICommandOneFlowBuilderVoid<TModel> BuildCommand(
        IStartOneCommandVoid<TModel> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<None> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var repository = unitOfWork.RepositoryOf<TModel>();
        var buildResult = BuildCommand(new CommandOneVoidFlow<TModel>(), requestContext);
        var commandType = buildResult.CommandTypeOne;
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
                break;
            case CommandTypeOne.Update:
                var itemUpdating = await repository
                    .GetFirstByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                if (itemUpdating is null) throw buildResult.NullError;

                if (buildResult.ConditionAsync is { } conditionUpdatingAsync)
                {
                    var conditionResult = await conditionUpdatingAsync.Invoke(itemUpdating);
                    conditionResult.ThrowIfError();
                }

                await buildResult.UpdateOneFunc.Invoke(itemUpdating);
                break;
            case CommandTypeOne.Remove:
                var itemRemoving = await repository.GetFirstByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                if (itemRemoving is null) throw buildResult.NullError;
                if (buildResult.ConditionAsync is { } conditionRemovingAsync)
                {
                    var conditionResult = await conditionRemovingAsync.Invoke(itemRemoving);
                    conditionResult.ThrowIfError();
                }

                await repository.RemoveOneAsync(itemRemoving, requestContext.CancellationToken);
                break;
            case CommandTypeOne.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        await unitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        return None.Value;
    }
}