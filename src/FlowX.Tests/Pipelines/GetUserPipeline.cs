using FlowX.Abstractions;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;

namespace FlowX.Tests.Pipelines;

public sealed class GetUserPipeline : IPipelineBehavior<GetUserQuery, UserResponse>
{
    public async Task<UserResponse> HandleAsync(IRequestContext<GetUserQuery> requestContext,
        Func<Task<UserResponse>> next)
    {
        Console.WriteLine("GetUserPipeline");
        var result = await next.Invoke();
        return result;
    }
}