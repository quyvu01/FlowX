using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Extensions;
using FlowX.Extensions;
using FlowX.Tests.DbContexts;
using FlowX.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowX.Tests.Tests;

public class FlowXEfCoreTest : ServicesBuilding
{
    public FlowXEfCoreTest()
    {
        InstallService((serviceCollection, _) => serviceCollection
                .AddDbContext<TestDbContext>(opts => opts
                    .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")))
            .InstallService((serviceCollection, _) =>
            {
                serviceCollection
                    .AddFlowX(cfg => cfg.AddHandlersFromNamespaceContaining<ITestAssemblyMarker>())
                    .AddEfCore(c => c.AddDbContexts(typeof(TestDbContext)));
            })
            .InstallAllServices();
        var dbContext = ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Users.AddRange(StaticData.StaticDataTest.Users);
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task Users_Counting_Should_Be_Equals_Static_Counting()
    {
        var userRepository = ServiceProvider.GetRequiredService<IRepository<User>>();
        var userCounting = await userRepository.CountByConditionAsync();
        Assert.Equal(StaticData.StaticDataTest.Users.Count, userCounting);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public async Task User_Filtered_Should_Have_The_Result(string userId)
    {
        var userRepository = ServiceProvider.GetRequiredService<IRepository<User>>();
        var user = await userRepository.GetFirstByConditionAsync(x => x.Id == userId);
        Assert.NotNull(user);
    }

    [Theory]
    [InlineData("4")]
    [InlineData("5")]
    public async Task User_Filtered_Should_Not_Be_Found(string userId)
    {
        var userRepository = ServiceProvider.GetRequiredService<IRepository<User>>();
        var user = await userRepository.GetFirstByConditionAsync(x => x.Id == userId);
        Assert.Null(user);
    }

    [Fact]
    public async Task User_Should_Be_Created()
    {
        var userRepository = ServiceProvider.GetRequiredService<IRepository<User>>();
        var unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
        var newUser = new User { Id = "4", Name = "Some one like you!", Email = "abc@gm.co" };
        await userRepository.CreateOneAsync(newUser);
        await unitOfWork.SaveChangesAsync();
        var userCreated = await userRepository.GetFirstByConditionAsync(a => a.Id == newUser.Id);
        Assert.Equal(newUser.Name, userCreated.Name);
    }
}