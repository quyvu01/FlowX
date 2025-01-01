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
    : ICommandHandler<TCommand, OneOf<TResult, ErrorDetail>>
    where TModel : class
    where TCommand : class, ICommand<OneOf<TResult, ErrorDetail>>
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandOneFlowBuilderResult<TModel, TResult> BuildCommand(
        IStartOneCommandResult<TModel, TResult> fromFlow, RequestContext<TCommand> command);

    public virtual async Task<OneOf<TResult, ErrorDetail>> HandleAsync(RequestContext<TCommand> requestContext)
    {
        var buildResult = BuildCommand(new CommandOneResultFlow<TModel, TResult>(), requestContext);
        var commandType = buildResult.CommandTypeOne;
        TModel item;
        switch (commandType)
        {
            case CommandTypeOne.Create:
                var createItem = await buildResult.ModelCreateFunc.Invoke();
                var createOneCondition = await buildResult.CommandOneCondition.Invoke(createItem);
                if (createOneCondition.IsT1) return createOneCondition.AsT1;
                await SqlRepository.CreateOneAsync(createItem, token: requestContext.CancellationToken);
                item = createItem;
                break;
            case CommandTypeOne.Update:
                var updateItem = await SqlRepository
                    .GetFirstByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                var updateManyCondition = await buildResult.CommandOneCondition.Invoke(updateItem);
                if (updateManyCondition.IsT1) return updateManyCondition.AsT1;
                await buildResult.UpdateOneFunc.Invoke(updateItem);
                item = updateItem;
                break;
            case CommandTypeOne.Remove:
                var removeItem = await SqlRepository.GetFirstByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                var removeManyCondition = await buildResult.CommandOneCondition.Invoke(removeItem);
                if (removeManyCondition.IsT1) return removeManyCondition.AsT1;
                await SqlRepository.RemoveOneAsync(removeItem, requestContext.CancellationToken);
                item = removeItem;
                break;
            case CommandTypeOne.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        var saveResult = await UnitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        if (saveResult.IsT1)
            return buildResult.SaveChangesErrorDetail;
        var result = buildResult.ResultFunc.Invoke(item);
        return result;
    }
}