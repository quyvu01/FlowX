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
    : ICommandHandler<TCommand, OneOf<TResult, ErrorDetail>>
    where TModel : class
    where TCommand : class, ICommand<OneOf<TResult, ErrorDetail>>
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandManyFlowBuilderResult<TModel, TResult> BuildCommand(
        IStartManyCommandResult<TModel, TResult> fromFlow, RequestContext<TCommand> requestContext);

    public virtual async Task<OneOf<TResult, ErrorDetail>> HandleAsync(RequestContext<TCommand> requestContext)
    {
        var buildResult = BuildCommand(new CommandManyResultFlow<TModel, TResult>(), requestContext);
        var commandType = buildResult.CommandTypeMany;
        List<TModel> items;
        switch (commandType)
        {
            case CommandTypeMany.Create:
                var createItems = await buildResult.ModelsCreateFunc.Invoke();
                var createManyCondition = await buildResult.CommandManyCondition.Invoke(createItems);
                if (createManyCondition.IsT1) return createManyCondition.AsT1;
                await SqlRepository.CreateManyAsync(createItems, token: requestContext.CancellationToken);
                items = createItems;
                break;
            case CommandTypeMany.Update:
                var updateItems = await SqlRepository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                var updateManyCondition = await buildResult.CommandManyCondition.Invoke(updateItems);
                if (updateManyCondition.IsT1) return updateManyCondition.AsT1;
                await buildResult.UpdateManyFunc.Invoke(updateItems);
                items = updateItems;
                break;
            case CommandTypeMany.Remove:
                var removeItems = await SqlRepository.GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                var removeManyCondition = await buildResult.CommandManyCondition.Invoke(removeItems);
                if (removeManyCondition.IsT1) return removeManyCondition.AsT1;
                await SqlRepository.RemoveManyAsync(removeItems, requestContext.CancellationToken);
                items = removeItems;
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        var saveResult = await UnitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        if (saveResult.IsT1)
            return buildResult.SaveChangesErrorDetail;
        var result = buildResult.ResultFunc.Invoke(items);
        return result;
    }
}