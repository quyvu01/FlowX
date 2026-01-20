using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Commands;
using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class FlowXMediatorTests
{
    #region IMediator Interface Tests

    [Fact]
    public void IMediator_Should_Define_Send_Methods()
    {
        var methods = typeof(IMediator).GetMethods().Where(m => m.Name == "Send").ToList();
        Assert.Equal(2, methods.Count);
    }

    [Fact]
    public void IMediator_Should_Have_Generic_Send_Method()
    {
        var methods = typeof(IMediator).GetMethods().Where(m => m.Name == "Send").ToList();
        Assert.Contains(methods, m => m.IsGenericMethodDefinition);
    }

    [Fact]
    public void IMediator_Should_Have_NonGeneric_Send_Method()
    {
        var methods = typeof(IMediator).GetMethods().Where(m => m.Name == "Send").ToList();
        Assert.Contains(methods, m => !m.IsGenericMethodDefinition);
    }

    #endregion

    #region Query Interface Tests

    [Fact]
    public void GetUserQuery_Should_Implement_IQueryOne()
    {
        var queryType = typeof(GetUserQuery);
        var interfaces = queryType.GetInterfaces();

        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryOne<>));
    }

    [Fact]
    public void GetUsersQuery_Should_Implement_IQueryCollection()
    {
        var queryType = typeof(GetUsersQuery);
        var interfaces = queryType.GetInterfaces();

        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryCollection<>));
    }

    [Fact]
    public void GetUserQuery_Should_Have_Id_Property()
    {
        var query = new GetUserQuery("test-id");
        Assert.Equal("test-id", query.Id);
    }

    [Fact]
    public void GetUsersQuery_Should_Have_Ids_Property()
    {
        var ids = new List<string> { "1", "2", "3" };
        var query = new GetUsersQuery(ids);
        Assert.Equal(ids, query.Ids);
    }

    #endregion

    #region Command Interface Tests

    [Fact]
    public void CreateUserCommand_Should_Implement_ICommandVoid()
    {
        var commandType = typeof(CreateUserCommand);
        var interfaces = commandType.GetInterfaces();

        Assert.Contains(interfaces, i => i == typeof(ICommandVoid));
    }

    [Fact]
    public void CreateUserCommand_Should_Have_Required_Properties()
    {
        var command = new CreateUserCommand("1", "TestUser", "test@test.com");

        Assert.Equal("1", command.Id);
        Assert.Equal("TestUser", command.Name);
        Assert.Equal("test@test.com", command.Email);
    }

    #endregion

    #region Response Tests

    [Fact]
    public void UserResponse_Should_Have_Required_Properties()
    {
        var response = new UserResponse
        {
            Id = "1",
            Name = "TestUser",
            Email = "test@test.com"
        };

        Assert.Equal("1", response.Id);
        Assert.Equal("TestUser", response.Name);
        Assert.Equal("test@test.com", response.Email);
    }

    #endregion

    #region IRequestContext Interface Tests

    [Fact]
    public void IRequestContext_Should_Define_Request_Property()
    {
        var property = typeof(IRequestContext<>).GetProperty("Request");
        Assert.NotNull(property);
    }

    [Fact]
    public void IRequestContext_Should_Inherit_From_IContext()
    {
        var interfaces = typeof(IRequestContext<>).GetInterfaces();
        Assert.Contains(interfaces, i => i == typeof(IContext));
    }

    [Fact]
    public void IContext_Should_Define_Headers_Property()
    {
        var property = typeof(IContext).GetProperty("Headers");
        Assert.NotNull(property);
    }

    [Fact]
    public void IContext_Should_Define_CancellationToken_Property()
    {
        var property = typeof(IContext).GetProperty("CancellationToken");
        Assert.NotNull(property);
    }

    #endregion

    #region IRequestHandler Interface Tests

    [Fact]
    public void IRequestHandler_Should_Define_HandleAsync()
    {
        var interfaceType = typeof(IRequestHandler<,>);
        var method = interfaceType.GetMethod("HandleAsync");

        Assert.NotNull(method);
    }

    [Fact]
    public void IQueryHandler_Should_Inherit_From_IRequestHandler()
    {
        var interfaces = typeof(IQueryHandler<,>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    }

    [Fact]
    public void ICommandHandler_Should_Inherit_From_IRequestHandler()
    {
        var interfaces = typeof(ICommandHandler<,>).GetInterfaces();
        Assert.Contains(interfaces, i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    }

    #endregion
}
