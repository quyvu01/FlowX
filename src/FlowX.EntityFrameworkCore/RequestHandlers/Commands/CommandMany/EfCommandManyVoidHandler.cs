using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;
using FlowX.EntityFrameworkCore.SharedStates;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;

public abstract class EfCommandManyVoidHandler<TModel, TCommand>
    : ICommandHandler<TCommand, None>
    where TModel : class
    where TCommand : class, ICommandVoid
{
    protected abstract ICommandManyFlowBuilderVoid<TModel> BuildCommand(
        IStartManyCommandVoid<TModel> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<None> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var unitOfWork = EfCoreSharedStates.GetUnitOfWork();
        var repository = unitOfWork.RepositoryOf<TModel>();
        var buildResult = BuildCommand(new CommandManyWithVoidFlow<TModel>(), requestContext);
        var commandType = buildResult.CommandTypeMany;
        switch (commandType)
        {
            case CommandTypeMany.Create:
                var itemsCreating = await buildResult.ModelsCreateFunc.Invoke();
                if (buildResult.ConditionAsync is { } conditionCreatingAsync)
                {
                    var errorResult = await conditionCreatingAsync.Invoke(itemsCreating);
                    if (errorResult.IsT1) throw errorResult.AsT1;
                }

                await repository.CreateManyAsync(itemsCreating, token: requestContext.CancellationToken);
                break;
            case CommandTypeMany.Update:
                var itemsUpdating = await repository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                if (buildResult.ConditionAsync is { } conditionUpdatingAsync)
                {
                    var errorResult = await conditionUpdatingAsync.Invoke(itemsUpdating);
                    if (errorResult.IsT1) throw errorResult.AsT1;
                }

                await buildResult.UpdateManyFunc.Invoke(itemsUpdating);
                break;
            case CommandTypeMany.Remove:
                var itemsRemoving = await repository.GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                if (buildResult.ConditionAsync is { } conditionRemovingAsync)
                {
                    var errorResult = await conditionRemovingAsync.Invoke(itemsRemoving);
                    if (errorResult.IsT1) throw errorResult.AsT1;
                }

                await repository.RemoveManyAsync(itemsRemoving, requestContext.CancellationToken);
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        await unitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        return None.Value;
    }
}