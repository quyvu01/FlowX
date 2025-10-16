using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;
using FlowX.EntityFrameworkCore.Abstractions;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;

public abstract class EfCommandManyResultHandler<TModel, TCommand, TResult>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, TResult>
    where TModel : class
    where TCommand : class, ICommandResult<TResult>
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandManyFlowBuilderResult<TModel, TResult> BuildCommand(
        IStartManyCommandResult<TModel, TResult> fromFlow, IRequestContext<TCommand> commandContext);

    public virtual async Task<TResult> HandleAsync(IRequestContext<TCommand> requestContext)
    {
        var buildResult = BuildCommand(new CommandManyResultFlow<TModel, TResult>(), requestContext);
        var commandType = buildResult.CommandTypeMany;
        List<TModel> items;
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
                items = itemsCreating;
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
                items = itemsUpdating;
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
                items = itemsRemoving;
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        await UnitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        var result = buildResult.ResultFunc.Invoke(items);
        return result;
    }
}