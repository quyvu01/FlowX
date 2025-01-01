using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;
using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;

public abstract class EfCommandManyVoidHandler<TModel, TCommand>(
    ISqlRepository<TModel> sqlRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, OneOf<None, ErrorDetail>>
    where TModel : class
    where TCommand : class, ICommand<OneOf<None, ErrorDetail>>
{
    protected ISqlRepository<TModel> SqlRepository { get; } = sqlRepository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    protected abstract ICommandManyFlowBuilderVoid<TModel> BuildCommand(
        IStartManyCommandVoid<TModel> fromFlow, RequestContext<TCommand> command);

    public virtual async Task<OneOf<None, ErrorDetail>> HandleAsync(RequestContext<TCommand> requestContext)
    {
        var buildResult = BuildCommand(new CommandManyWithVoidFlow<TModel>(), requestContext);
        var commandType = buildResult.CommandTypeMany;
        switch (commandType)
        {
            case CommandTypeMany.Create:
                var createItems = await buildResult.ModelsCreateFunc.Invoke();
                var createManyCondition = await buildResult.CommandManyCondition.Invoke(createItems);
                if (createManyCondition.IsT1) return createManyCondition.AsT1;
                await SqlRepository.CreateManyAsync(createItems, token: requestContext.CancellationToken);
                break;
            case CommandTypeMany.Update:
                var updateItems = await SqlRepository
                    .GetManyByConditionAsync(buildResult.CommandFilter, buildResult.CommandSpecialAction,
                        token: requestContext.CancellationToken);
                var updateManyCondition = await buildResult.CommandManyCondition.Invoke(updateItems);
                if (updateManyCondition.IsT1) return updateManyCondition.AsT1;
                await buildResult.UpdateManyFunc.Invoke(updateItems);
                break;
            case CommandTypeMany.Remove:
                var removeItems = await SqlRepository.GetManyByConditionAsync(buildResult.CommandFilter,
                    buildResult.CommandSpecialAction, token: requestContext.CancellationToken);
                var removeManyCondition = await buildResult.CommandManyCondition.Invoke(removeItems);
                if (removeManyCondition.IsT1) return removeManyCondition.AsT1;
                await SqlRepository.RemoveManyAsync(removeItems, requestContext.CancellationToken);
                break;
            case CommandTypeMany.Unknown:
            default:
                throw new UnreachableException($"Command {commandType} does not support!");
        }

        var saveResult = await UnitOfWork.SaveChangesAsync(requestContext.CancellationToken);
        if (saveResult.IsT1)
            return buildResult.SaveChangesErrorDetail;
        return None.Value;
    }
}