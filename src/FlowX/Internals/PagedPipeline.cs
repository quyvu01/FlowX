using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Responses;

namespace FlowX.Internals;

internal class PagedPipeline<TRequest, TResult> : IFlowPipelineBehavior<TRequest, TResult>
    where TRequest : GetManyQuery, IRequest<TResult>
    where TResult : PaginationResponseGeneral
{
    public async Task<TResult> HandleAsync(IRequestContext<TRequest> requestContext, Func<Task<TResult>> next)
    {
        var result = await next.Invoke();
        var take = requestContext.Request.Take();
        var totalRecord = result.TotalRecord;
        var totalPage = take is null or <= 0 ? 1 : (int)(totalRecord + take.Value - 1) / take.Value;
        result.TotalPage = totalPage;
        result.CurrentPageIndex = requestContext.Request.PageIndex ?? 1;
        return result;
    }
}