using FlowX.Abstractions;
using FlowX.EntityFrameworkCore.Extensions;
using FlowX.Extensions;
using FlowX.Nats.Extensions;
using FlowX.Tests.DbContexts;
using FlowX.Tests.Pipelines;
using FlowX.Tests.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class FlowXEfFlowTest : ServicesBuilding
{
    public FlowXEfFlowTest()
    {
        InstallService((serviceCollection, _) => serviceCollection
                .AddDbContext<TestDbContext>(opts => opts
                    .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")))
            .InstallService((serviceCollection, _) =>
            {
                serviceCollection.AddFlowX(cfg =>
                {
                    cfg.AddModelsFromNamespaceContaining<ITestAssemblyMarker>();
                    cfg.AddHandlersFromNamespaceContaining<ITestAssemblyMarker>();
                    cfg.AddDbContextDynamic<TestDbContext>(options =>
                    {
                        options.AddDynamicRepositories();
                        options.AddDynamicUnitOfWork();
                    });
                    cfg.AddPipelines(c => c.OfType<GetUserPipeline>(ServiceLifetime.Transient));
                    cfg.AddNats(config => config.Url("nats://localhost:4222"));
                });
            })
            .InstallAllServices();
        var dbContext = ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Users.AddRange(StaticData.StaticDataTest.Users);
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task User_Should_Has_Result_With_Ef_Request()
    {
        var sender = ServiceProvider.GetRequiredService<IFlow>();
        var userResult = await sender.Send(new GetUserQuery("1"));
        Assert.True(userResult.IsT0);
    }

    [Theory]
    [InlineData("1", "2")]
    public async Task Users_Filtered_Should_Have_Data(params string[] ids)
    {
        var sender = ServiceProvider.GetRequiredService<IFlow>();
        var userResult = await sender.Send(new GetUsersQuery([..ids]));
        Assert.Equal(ids.Length, userResult.Items.Count);
    }

    [Fact]
    public async Task User_Should_Be_Created()
    {
        var sender = ServiceProvider.GetRequiredService<IFlow>();
        var userCreatedCommand = new CreateUserCommand("4", "Abc", "ac@gm.co");
        var userResult = await sender.Send(new CreateUserCommand("4", "Abc", "ac@gm.co"));
        Assert.True(userResult.IsT0);
        var newUser = await sender.Send(new GetUserQuery(userCreatedCommand.Id));
        Assert.True(newUser.IsT0);
        Assert.Equal(newUser.AsT0.Email, userCreatedCommand.Email);
    }
}