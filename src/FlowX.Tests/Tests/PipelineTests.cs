using FlowX.Abstractions;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class PipelineTests
{
    [Fact]
    public void PipelineBehavior_Interface_Should_Have_HandleAsync_Method()
    {
        var interfaceType = typeof(IPipelineBehavior<,>);
        var method = interfaceType.GetMethod("HandleAsync");

        Assert.NotNull(method);
        Assert.Equal("HandleAsync", method.Name);
    }

    [Fact]
    public void PipelineBehavior_Generic_Parameters_Should_Be_Correct()
    {
        var interfaceType = typeof(IPipelineBehavior<,>);
        var genericArgs = interfaceType.GetGenericArguments();

        Assert.Equal(2, genericArgs.Length);
    }

    [Fact]
    public void TestLoggingPipeline_Should_Implement_IPipelineBehavior()
    {
        var pipelineType = typeof(TestLoggingPipeline);
        var implementedInterface = typeof(IPipelineBehavior<GetUserQuery, UserResponse>);

        Assert.True(implementedInterface.IsAssignableFrom(pipelineType));
    }

    [Fact]
    public async Task TestLoggingPipeline_Should_Execute_Next_And_Log()
    {
        var log = new List<string>();
        var pipeline = new TestLoggingPipeline(log);
        var expectedResponse = new UserResponse { Id = "1", Name = "Test", Email = "test@test.com" };

        var requestContext = new TestRequestContext<GetUserQuery>(
            new GetUserQuery("1"),
            new Dictionary<string, string>(),
            CancellationToken.None);

        var result = await pipeline.HandleAsync(requestContext, () => Task.FromResult(expectedResponse));

        Assert.Equal(expectedResponse, result);
        Assert.Contains("TestLoggingPipeline:Before", log);
        Assert.Contains("TestLoggingPipeline:After", log);
        Assert.Contains("RequestId:1", log);
        Assert.Contains("Response:Test", log);
    }

    [Fact]
    public async Task Pipeline_Should_Execute_Before_Then_After()
    {
        var log = new List<string>();
        var pipeline = new TestLoggingPipeline(log);
        var response = new UserResponse { Id = "1", Name = "User", Email = "user@test.com" };

        var requestContext = new TestRequestContext<GetUserQuery>(
            new GetUserQuery("1"),
            new Dictionary<string, string>(),
            CancellationToken.None);

        await pipeline.HandleAsync(requestContext, () => Task.FromResult(response));

        var beforeIndex = log.IndexOf("TestLoggingPipeline:Before");
        var afterIndex = log.IndexOf("TestLoggingPipeline:After");
        Assert.True(beforeIndex < afterIndex);
    }

    [Fact]
    public async Task Pipeline_Should_Pass_Through_Response_Unchanged()
    {
        var log = new List<string>();
        var pipeline = new TestLoggingPipeline(log);
        var expectedResponse = new UserResponse { Id = "123", Name = "OriginalName", Email = "original@test.com" };

        var requestContext = new TestRequestContext<GetUserQuery>(
            new GetUserQuery("123"),
            new Dictionary<string, string>(),
            CancellationToken.None);

        var result = await pipeline.HandleAsync(requestContext, () => Task.FromResult(expectedResponse));

        Assert.Same(expectedResponse, result);
    }

    [Fact]
    public async Task Pipeline_Should_Handle_Exception_From_Next()
    {
        var log = new List<string>();
        var pipeline = new TestLoggingPipeline(log);

        var requestContext = new TestRequestContext<GetUserQuery>(
            new GetUserQuery("1"),
            new Dictionary<string, string>(),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.HandleAsync(requestContext, () => throw new InvalidOperationException("Test exception")));
    }
}

public sealed class TestLoggingPipeline(List<string> log) : IPipelineBehavior<GetUserQuery, UserResponse>
{
    public async Task<UserResponse> HandleAsync(
        IRequestContext<GetUserQuery> requestContext,
        Func<Task<UserResponse>> next)
    {
        log.Add("TestLoggingPipeline:Before");
        log.Add($"RequestId:{requestContext.Request.Id}");

        var result = await next.Invoke();

        log.Add($"Response:{result.Name}");
        log.Add("TestLoggingPipeline:After");

        return result;
    }
}

internal sealed class TestRequestContext<TRequest>(
    TRequest request,
    Dictionary<string, string> headers,
    CancellationToken cancellationToken) : IRequestContext<TRequest>
{
    public TRequest Request { get; } = request;
    public Dictionary<string, string> Headers { get; } = headers;
    public CancellationToken CancellationToken { get; } = cancellationToken;
}
