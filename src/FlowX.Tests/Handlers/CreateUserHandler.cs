using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;
using FlowX.Errors;
using FlowX.Structs;
using FlowX.Tests.Models;
using FlowX.Tests.Requests;

namespace FlowX.Tests.Handlers;

public sealed class CreateUserHandler(ISqlRepository<User> sqlRepository, IUnitOfWork unitOfWork)
    : EfCommandOneVoidHandler<User, CreateUserCommand>(sqlRepository, unitOfWork)
{
    protected override ICommandOneFlowBuilderVoid<User> BuildCommand(IStartOneCommandVoid<User> fromFlow,
        IRequestContext<CreateUserCommand> commandContext)
        => fromFlow
            .CreateOne(new User
            {
                Id = commandContext.Request.Id, Name = commandContext.Request.Name, Email = commandContext.Request.Email
            })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error { Code = "123", Messages = ["Create Failed!"] });
}