using FlowX.Abstractions;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.SharedStates;

namespace FlowX.EntityFrameworkCore.InternalPipelines;

internal sealed class UnitOfWorkStatePipeline<TRequest, TResult>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResult> where TRequest : IRequest<TResult>
{
    public async Task<TResult> HandleAsync(IRequestContext<TRequest> requestContext, Func<Task<TResult>> next)
    {
        EfCoreSharedStates.CreateContext(unitOfWork);
        var result = await next.Invoke();
        return result;
    }
}