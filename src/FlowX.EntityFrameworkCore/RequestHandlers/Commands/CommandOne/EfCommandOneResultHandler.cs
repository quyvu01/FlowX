using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;

public abstract class EfCommandOneResultHandler<TModel, TCommand, TResult>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, OneOf<TResult, Error>>
    where TModel : class
    where TCommand : class, ICommandResult<TResult>
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandOneFlowBuilderResult<TModel, TResult> BuildCommand(
        IStartOneCommandResult<TModel, TResult> fromFlow, IRequestXContext<TCommand> commandXContext);

    public virtual async Task<OneOf<TResult, Error>> HandleAsync(IRequestXContext<TCommand> requestXContext)
    {
        var buildResult = BuildCommand(new CommandOneResultFlow<TModel, TResult>(), requestXContext);
        var commandType = buildResult.CommandTypeOne;
        TModel item;
        switch (commandType)
        {
            case CommandTypeOne.Create:
                var createItem = await buildResult.ModelCreateFunc.Invoke();
                var createOneCondition = await buildResult.CommandOneCondition.Invoke(createItem);
                if (createOneCondition.IsT1) return createOneCondition.AsT1;
                await SqlRepository.CreateOneAsync(createItem, token: requestXContext.CancellationToken);
                item = createItem;
                break;
            case CommandTypeOne.Update:
                var updateItem = await SqlRepository
                    .GetFirstByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestXContext.CancellationToken);
                var updateManyCondition = await buildResult.CommandOneCondition.Invoke(updateItem);
                if (updateManyCondition.IsT1) return updateManyCondition.AsT1;
                await buildResult.UpdateOneFunc.Invoke(updateItem);
                item = updateItem;
                break;
            case CommandTypeOne.Remove:
                var removeItem = await SqlRepository.GetFirstByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestXContext.CancellationToken);
                var removeManyCondition = await buildResult.CommandOneCondition.Invoke(removeItem);
                if (removeManyCondition.IsT1) return removeManyCondition.AsT1;
                await SqlRepository.RemoveOneAsync(removeItem, requestXContext.CancellationToken);
                item = removeItem;
                break;
            case CommandTypeOne.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        var saveResult = await UnitOfWork.SaveChangesAsync(requestXContext.CancellationToken);
        if (saveResult.IsT1)
            return buildResult.SaveChangesError;
        var result = buildResult.ResultFunc.Invoke(item);
        return result;
    }
}