using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;

public abstract class EfCommandManyResultHandler<TModel, TCommand, TResult>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, OneOf<TResult, Error>>
    where TModel : class
    where TCommand : class, ICommandResult<TResult>
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandManyFlowBuilderResult<TModel, TResult> BuildCommand(
        IStartManyCommandResult<TModel, TResult> fromFlow, IRequestXContext<TCommand> commandXContext);

    public virtual async Task<OneOf<TResult, Error>> HandleAsync(IRequestXContext<TCommand> requestXContext)
    {
        var buildResult = BuildCommand(new CommandManyResultFlow<TModel, TResult>(), requestXContext);
        var commandType = buildResult.CommandTypeMany;
        List<TModel> items;
        switch (commandType)
        {
            case CommandTypeMany.Create:
                var createItems = await buildResult.ModelsCreateFunc.Invoke();
                var createManyCondition = await buildResult.CommandManyCondition.Invoke(createItems);
                if (createManyCondition.IsT1) return createManyCondition.AsT1;
                await SqlRepository.CreateManyAsync(createItems, token: requestXContext.CancellationToken);
                items = createItems;
                break;
            case CommandTypeMany.Update:
                var updateItems = await SqlRepository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestXContext.CancellationToken);
                var updateManyCondition = await buildResult.CommandManyCondition.Invoke(updateItems);
                if (updateManyCondition.IsT1) return updateManyCondition.AsT1;
                await buildResult.UpdateManyFunc.Invoke(updateItems);
                items = updateItems;
                break;
            case CommandTypeMany.Remove:
                var removeItems = await SqlRepository.GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestXContext.CancellationToken);
                var removeManyCondition = await buildResult.CommandManyCondition.Invoke(removeItems);
                if (removeManyCondition.IsT1) return removeManyCondition.AsT1;
                await SqlRepository.RemoveManyAsync(removeItems, requestXContext.CancellationToken);
                items = removeItems;
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        var saveResult = await UnitOfWork.SaveChangesAsync(requestXContext.CancellationToken);
        if (saveResult.IsT1)
            return buildResult.SaveChangesError;
        var result = buildResult.ResultFunc.Invoke(items);
        return result;
    }
}