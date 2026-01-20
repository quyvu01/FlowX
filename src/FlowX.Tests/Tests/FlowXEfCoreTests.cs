using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.Tests.DbContexts;
using FlowX.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class FlowXEfCoreTests
{
    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var context = new TestDbContext(options);
        context.Users.AddRange(StaticData.StaticDataTest.Users);
        context.SaveChanges();
        return context;
    }

    #region DbContext Tests

    [Fact]
    public async Task DbContext_Users_Should_Have_Seeded_Data()
    {
        await using var dbContext = CreateDbContext();

        var count = await dbContext.Users.CountAsync();

        Assert.Equal(StaticData.StaticDataTest.Users.Count, count);
    }

    [Theory]
    [InlineData("1", "user1")]
    [InlineData("2", "user2")]
    [InlineData("3", "user3")]
    public async Task DbContext_Users_Should_Return_Correct_User(string id, string expectedName)
    {
        await using var dbContext = CreateDbContext();

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

        Assert.NotNull(user);
        Assert.Equal(expectedName, user.Name);
    }

    [Fact]
    public async Task DbContext_Should_Add_New_User()
    {
        await using var dbContext = CreateDbContext();
        var newUser = new User { Id = "4", Name = "NewUser", Email = "new@gm.com" };

        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync();

        var count = await dbContext.Users.CountAsync();
        Assert.Equal(StaticData.StaticDataTest.Users.Count + 1, count);
    }

    [Fact]
    public async Task DbContext_Should_Remove_User()
    {
        await using var dbContext = CreateDbContext();
        var user = await dbContext.Users.FirstAsync(x => x.Id == "1");

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        var deletedUser = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == "1");
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task DbContext_Should_Update_User()
    {
        await using var dbContext = CreateDbContext();
        var user = await dbContext.Users.FirstAsync(x => x.Id == "1");

        user.Name = "UpdatedName";
        await dbContext.SaveChangesAsync();

        var updatedUser = await dbContext.Users.FirstAsync(x => x.Id == "1");
        Assert.Equal("UpdatedName", updatedUser.Name);
    }

    #endregion

    #region IRepository Interface Tests

    [Fact]
    public void IRepository_Should_Define_GetQueryable()
    {
        var method = typeof(IRepository<>).GetMethod("GetQueryable");
        Assert.NotNull(method);
    }

    [Fact]
    public void IRepository_Should_Define_GetFirstByConditionAsync()
    {
        var method = typeof(IRepository<>).GetMethod("GetFirstByConditionAsync");
        Assert.NotNull(method);
    }

    [Fact]
    public void IRepository_Should_Define_ExistByConditionAsync()
    {
        var method = typeof(IRepository<>).GetMethod("ExistByConditionAsync");
        Assert.NotNull(method);
    }

    [Fact]
    public void IRepository_Should_Define_CountByConditionAsync()
    {
        var method = typeof(IRepository<>).GetMethod("CountByConditionAsync");
        Assert.NotNull(method);
    }

    [Fact]
    public void IRepository_Should_Define_CreateOneAsync()
    {
        var method = typeof(IRepository<>).GetMethod("CreateOneAsync");
        Assert.NotNull(method);
    }

    [Fact]
    public void IRepository_Should_Define_CreateManyAsync()
    {
        var method = typeof(IRepository<>).GetMethod("CreateManyAsync");
        Assert.NotNull(method);
    }

    [Fact]
    public void IRepository_Should_Define_RemoveOneAsync_WithFilter()
    {
        var methods = typeof(IRepository<>).GetMethods()
            .Where(m => m.Name == "RemoveOneAsync")
            .ToList();
        Assert.Equal(2, methods.Count);
    }

    #endregion

    #region IUnitOfWork Interface Tests

    [Fact]
    public void IUnitOfWork_Should_Define_SaveChangesAsync()
    {
        var method = typeof(IUnitOfWork).GetMethod("SaveChangesAsync");
        Assert.NotNull(method);
    }

    #endregion

    #region Query Tests with DbContext

    [Fact]
    public async Task Query_Should_Filter_By_Email()
    {
        await using var dbContext = CreateDbContext();

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == "user2@gm.com");

        Assert.NotNull(user);
        Assert.Equal("2", user.Id);
    }

    [Fact]
    public async Task Query_Should_Return_Empty_For_NonExisting()
    {
        await using var dbContext = CreateDbContext();

        var users = await dbContext.Users.Where(x => x.Id == "999").ToListAsync();

        Assert.Empty(users);
    }

    [Fact]
    public async Task Query_Projection_Should_Work()
    {
        await using var dbContext = CreateDbContext();

        var emails = await dbContext.Users.Select(x => x.Email).ToListAsync();

        Assert.Equal(3, emails.Count);
        Assert.All(emails, e => Assert.Contains("@gm.com", e));
    }

    [Fact]
    public async Task Query_Ordering_Should_Work()
    {
        await using var dbContext = CreateDbContext();

        var users = await dbContext.Users.OrderByDescending(x => x.Name).ToListAsync();

        Assert.Equal("user3", users[0].Name);
        Assert.Equal("user1", users[2].Name);
    }

    #endregion
}
