using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;
using FlowX.Errors;
using FlowX.Structs;
using Service1.Contracts.Requests;
using Service1.Models;

namespace Service1.Handlers;

public sealed class CreateUserHandler(ISqlRepository<User> sqlRepository, IUnitOfWork unitOfWork)
    : EfCommandOneVoidHandler<User, CreateUserCommand>(sqlRepository, unitOfWork)
{
    protected override ICommandOneFlowBuilderVoid<User> BuildCommand(IStartOneCommandVoid<User> fromFlow,
        RequestContext<CreateUserCommand> commandContext)
        => fromFlow
            .CreateOne(new User { Id = Guid.NewGuid(), Name = commandContext.Request.Name })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error { Code = "SomeError", Messages = ["Create user failed!"] });
}