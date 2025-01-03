using FlowX.Abstractions;
using FlowX.Errors;
using FlowX.Structs;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;

namespace FlowX.Tests.Pipelines;

public sealed class GetUserPipeline : ISqlPipelineBehavior<GetUserQuery, OneOf<UserResponse, Error>>
{
    public async Task<OneOf<UserResponse, Error>> HandleAsync(RequestContext<GetUserQuery> requestContext,
        Func<Task<OneOf<UserResponse, Error>>> next)
    {
        Console.WriteLine("GetUserPipeline");
        var result = await next.Invoke();
        return result;
    }
}