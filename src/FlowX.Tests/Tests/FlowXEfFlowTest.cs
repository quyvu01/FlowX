using FlowX.Abstractions;
using FlowX.EntityFrameworkCore.Extensions;
using FlowX.Errors;
using FlowX.Extensions;
using FlowX.Structs;
using FlowX.Tests.DbContexts;
using FlowX.Tests.Pipelines;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;
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
                    cfg.AddSqlPipelines(c => c.OfType<GetUserPipeline>(ServiceLifetime.Transient));
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
        var sender = ServiceProvider.GetRequiredService<IFlowXSender>();
        var userResult = await sender.ExecuteAsync(new GetUserQuery("1"));
        Assert.True(userResult.IsT0);
    }

    [Theory]
    [InlineData("1", "2")]
    public async Task Users_Filtered_Should_Have_Data(params string[] ids)
    {
        var sender = ServiceProvider.GetRequiredService<IFlowXSender>();
        var userResult = await sender.ExecuteAsync(new GetUsersQuery([..ids]));
        Assert.Equal(ids.Length, userResult.Items.Count);
    }
}