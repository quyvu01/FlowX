using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.Pipeline;
using FlowX.Errors;
using Service2.Contracts.Requests;
using Service2.Models;

namespace Service2.Handlers;

public sealed class CreateProvinceHandler : EfCommandPipelineHandler<CreateProvinceCommand>
{
    protected override IPipelineFlowBuilder BuildPipeline(IStartCommandPipeline fromFlow,
        IRequestContext<CreateProvinceCommand> commandContext)
        => fromFlow
            .CreateOne(new Province { Id = Guid.NewGuid(), Name = commandContext.Request.Name })
            .WithErrorIfSaveChange(new Error { Code = "SomeError", Messages = ["Create user failed!"] });
}