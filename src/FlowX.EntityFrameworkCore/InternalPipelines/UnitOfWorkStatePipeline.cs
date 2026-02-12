using FlowX.Abstractions;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.SharedStates;

namespace FlowX.EntityFrameworkCore.InternalPipelines;

internal sealed class UnitOfWorkStatePipeline<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public UnitOfWorkStatePipeline(IUnitOfWork unitOfWork) => EfCoreSharedStates.CreateContext(unitOfWork);

    public async Task<TResult> HandleAsync(IRequestContext<TRequest> requestContext, Func<Task<TResult>> next)
    {
        var result = await next.Invoke();
        return result;
    }
}

internal sealed class UnitOfWorkStatePipeline<TRequest> : IPipelineBehavior<TRequest> where TRequest : IRequest
{
    public UnitOfWorkStatePipeline(IUnitOfWork unitOfWork) => EfCoreSharedStates.CreateContext(unitOfWork);

    public async Task HandleAsync(IRequestContext<TRequest> requestContext, Func<Task> next) => await next.Invoke();
}