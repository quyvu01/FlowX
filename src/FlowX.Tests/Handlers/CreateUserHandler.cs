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
        IRequestXContext<CreateUserCommand> commandXContext)
        => fromFlow
            .CreateOne(new User
            {
                Id = commandXContext.Request.Id, Name = commandXContext.Request.Name, Email = commandXContext.Request.Email
            })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error { Code = "123", Messages = ["Create Failed!"] });
}