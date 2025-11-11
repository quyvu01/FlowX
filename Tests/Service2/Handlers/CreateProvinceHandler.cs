using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;
using FlowX.Errors;
using FlowX.Structs;
using Service2.Contracts.Requests;
using Service2.Models;

namespace Service2.Handlers;

public sealed class CreateProvinceHandler : EfCommandOneVoidHandler<Province, CreateProvinceCommand>
{
    protected override ICommandOneFlowBuilderVoid<Province> BuildCommand(IStartOneCommandVoid<Province> fromFlow,
        IRequestContext<CreateProvinceCommand> commandContext)
        => fromFlow
            .CreateOne(new Province { Id = Guid.NewGuid(), Name = commandContext.Request.Name })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(new Error { Code = "SomeError", Messages = ["Create user failed!"] });
}