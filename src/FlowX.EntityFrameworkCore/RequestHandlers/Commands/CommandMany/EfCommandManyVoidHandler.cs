using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;

public abstract class EfCommandManyVoidHandler<TModel, TCommand>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, OneOf<None, Error>>
    where TModel : class
    where TCommand : class, ICommandVoid
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandManyFlowBuilderVoid<TModel> BuildCommand(
        IStartManyCommandVoid<TModel> fromFlow, IRequestXContext<TCommand> commandXContext);

    public virtual async Task<OneOf<None, Error>> HandleAsync(IRequestXContext<TCommand> requestXContext)
    {
        var buildResult = BuildCommand(new CommandManyWithVoidFlow<TModel>(), requestXContext);
        var commandType = buildResult.CommandTypeMany;
        switch (commandType)
        {
            case CommandTypeMany.Create:
                var createItems = await buildResult.ModelsCreateFunc.Invoke();
                var createManyCondition = await buildResult.CommandManyCondition.Invoke(createItems);
                if (createManyCondition.IsT1) return createManyCondition.AsT1;
                await SqlRepository.CreateManyAsync(createItems, token: requestXContext.CancellationToken);
                break;
            case CommandTypeMany.Update:
                var updateItems = await SqlRepository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestXContext.CancellationToken);
                var updateManyCondition = await buildResult.CommandManyCondition.Invoke(updateItems);
                if (updateManyCondition.IsT1) return updateManyCondition.AsT1;
                await buildResult.UpdateManyFunc.Invoke(updateItems);
                break;
            case CommandTypeMany.Remove:
                var removeItems = await SqlRepository.GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestXContext.CancellationToken);
                var removeManyCondition = await buildResult.CommandManyCondition.Invoke(removeItems);
                if (removeManyCondition.IsT1) return removeManyCondition.AsT1;
                await SqlRepository.RemoveManyAsync(removeItems, requestXContext.CancellationToken);
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        var saveResult = await UnitOfWork.SaveChangesAsync(requestXContext.CancellationToken);
        if (saveResult.IsT1)
            return buildResult.SaveChangesError;
        return None.Value;
    }
}