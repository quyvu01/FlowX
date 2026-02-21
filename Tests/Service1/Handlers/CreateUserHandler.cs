using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.Pipeline;
using FlowX.Errors;
using Service1.Contracts.Requests;
using Service1.Models;

namespace Service1.Handlers;

public sealed class CreateUserHandler : EfCommandPipelineHandler<CreateUserCommand, string>
{
    protected override IPipelineResultFlowBuilder<string> BuildPipeline(IStartCommandPipeline fromFlow,
        IRequestContext<CreateUserCommand> commandContext)
        => fromFlow
            .CreateOne(new User { Id = Guid.NewGuid(), Name = "Abcd" })
            .ThenCreateOne(_ => new User { Id = Guid.NewGuid(), Name = "Xyz" })
            .WithModify(x => x.Name = "New Name")
            .WithResultIfSucceed(u => u.Id.ToString())
            .WithErrorIfSaveChange(new Error("Some error"));
}