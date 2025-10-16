using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.ApplicationModels;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;

public abstract class EfCommandOneVoidHandler<TModel, TCommand>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, None>
    where TModel : class
    where TCommand : class, ICommandVoid
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandOneFlowBuilderVoid<TModel> BuildCommand(
        IStartOneCommandVoid<TModel> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<None> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var buildResult = BuildCommand(new CommandOneVoidFlow<TModel>(), requestContext);
        var commandType = buildResult.CommandTypeOne;
        switch (commandType)
        {
            case CommandTypeOne.Create:
                var itemCreating = await buildResult.ModelCreateFunc.Invoke();
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemCreating);
                    throw errorResult;
                }

                await SqlRepository.CreateOneAsync(itemCreating, token: requestContext.CancellationToken);
                break;
            case CommandTypeOne.Update:
                var itemUpdating = await SqlRepository
                    .GetFirstByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                if (itemUpdating is null) throw buildResult.NullError;
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemUpdating);
                    throw errorResult;
                }

                await buildResult.UpdateOneFunc.Invoke(itemUpdating);
                break;
            case CommandTypeOne.Remove:
                var itemRemoving = await SqlRepository.GetFirstByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                if (itemRemoving is null) throw buildResult.NullError;
                if (buildResult.CommandConditionResultError is not null)
                {
                    var errorResult = await buildResult.CommandConditionResultError.Invoke(itemRemoving);
                    throw errorResult;
                }

                await SqlRepository.RemoveOneAsync(itemRemoving, requestContext.CancellationToken);
                break;
            case CommandTypeOne.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        await UnitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        return None.Value;
    }
}