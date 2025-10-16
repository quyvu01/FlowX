using FlowX.Abstractions;
using FlowX.Errors;
using FlowX.Structs;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;

namespace FlowX.Tests.Pipelines;

public sealed class GetUserPipeline : IFlowPipelineBehavior<GetUserQuery, UserResponse>
{
    public async Task<UserResponse> HandleAsync(IRequestContext<GetUserQuery> requestContext,
        Func<Task<UserResponse>> next)
    {
        Console.WriteLine("GetUserPipeline");
        var result = await next.Invoke();
        return result;
    }
}