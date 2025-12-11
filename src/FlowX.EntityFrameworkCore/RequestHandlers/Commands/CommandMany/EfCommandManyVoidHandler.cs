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
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemsCreating);
                    throw errorResult;
                }

                if (buildResult.CommandConditionResultNone is not null)
                    await buildResult.CommandConditionResultNone.Invoke(itemsCreating);

                await repository.CreateManyAsync(itemsCreating, token: requestContext.CancellationToken);
                break;
            case CommandTypeMany.Update:
                var itemsUpdating = await repository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemsUpdating);
                    throw errorResult;
                }

                if (buildResult.CommandConditionResultNone is not null)
                    await buildResult.CommandConditionResultNone.Invoke(itemsUpdating);

                await buildResult.UpdateManyFunc.Invoke(itemsUpdating);
                break;
            case CommandTypeMany.Remove:
                var itemsRemoving = await repository.GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemsRemoving);
                    throw errorResult;
                }

                if (buildResult.CommandConditionResultNone is not null)
                    await buildResult.CommandConditionResultNone.Invoke(itemsRemoving);

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