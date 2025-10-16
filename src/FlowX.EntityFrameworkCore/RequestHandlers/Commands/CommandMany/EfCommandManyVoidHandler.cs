using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;

public abstract class EfCommandManyVoidHandler<TModel, TCommand>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, None>
    where TModel : class
    where TCommand : class, ICommandVoid
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandManyFlowBuilderVoid<TModel> BuildCommand(
        IStartManyCommandVoid<TModel> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<None> HandleAsync(IRequestContext<TCommand> requestContext)
    {
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

                await SqlRepository.CreateManyAsync(itemsCreating, token: requestContext.CancellationToken);
                break;
            case CommandTypeMany.Update:
                var itemsUpdating = await SqlRepository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemsUpdating);
                    throw errorResult;
                }

                await buildResult.UpdateManyFunc.Invoke(itemsUpdating);
                break;
            case CommandTypeMany.Remove:
                var itemsRemoving = await SqlRepository.GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemsRemoving);
                    throw errorResult;
                }

                await SqlRepository.RemoveManyAsync(itemsRemoving, requestContext.CancellationToken);
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        await UnitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        return None.Value;
    }
}