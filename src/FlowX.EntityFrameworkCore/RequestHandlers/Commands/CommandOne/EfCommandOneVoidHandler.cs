using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;

public abstract class EfCommandOneVoidHandler<TModel, TCommand>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, OneOf<None, Error>>
    where TModel : class
    where TCommand : class, ICommandVoid
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandOneFlowBuilderVoid<TModel> BuildCommand(
        IStartOneCommandVoid<TModel> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<OneOf<None, Error>> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var buildResult = BuildCommand(new CommandOneVoidFlow<TModel>(), requestContext);
        var commandType = buildResult.CommandTypeOne;
        switch (commandType)
        {
            case CommandTypeOne.Create:
                var createItem = await buildResult.ModelCreateFunc.Invoke();
                var createManyCondition = await buildResult.CommandOneCondition.Invoke(createItem);
                if (createManyCondition.IsT1) return createManyCondition.AsT1;
                await SqlRepository.CreateOneAsync(createItem, token: requestContext.CancellationToken);
                break;
            case CommandTypeOne.Update:
                var updateItem = await SqlRepository
                    .GetFirstByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                if (updateItem is null) return buildResult.NullError;
                var updateOneCondition = await buildResult.CommandOneCondition.Invoke(updateItem);
                if (updateOneCondition.IsT1) return updateOneCondition.AsT1;
                await buildResult.UpdateOneFunc.Invoke(updateItem);
                break;
            case CommandTypeOne.Remove:
                var removeItem = await SqlRepository.GetFirstByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                if (removeItem is null) return buildResult.NullError;
                var removeOneCondition = await buildResult.CommandOneCondition.Invoke(removeItem);
                if (removeOneCondition.IsT1) return removeOneCondition.AsT1;
                await SqlRepository.RemoveOneAsync(removeItem, requestContext.CancellationToken);
                break;
            case CommandTypeOne.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        var saveResult = await UnitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        if (saveResult.IsT1)
            return buildResult.SaveChangesError;
        return None.Value;
    }
}