using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.Pipeline;
using FlowX.Errors;
using FlowX.Structs;
using Service1.Contracts.Requests;
using Service1.Models;

namespace Service1.Handlers;

public sealed class CreateUserHandler : EfPipelineVoidHandler<CreateUserCommand>
{
    protected override IPipelineFlowBuilder BuildPipeline(IStartPipeline fromFlow,
        IRequestContext<CreateUserCommand> commandContext)
        => fromFlow
            .CreateOne(new User { Id = Guid.NewGuid(), Name = "Abcd" })
            .ThenCreateOne(_ => new User { Id = Guid.NewGuid(), Name = "Xyz" })
            .Done()
            .WithErrorIfSaveChange(new Error());
}