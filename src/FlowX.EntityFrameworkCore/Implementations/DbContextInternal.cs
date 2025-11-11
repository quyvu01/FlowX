using FlowX.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Implementations;

internal sealed class DbContextInternal(DbContext dbContext) : IDbContext
{
    public DbContext DbContext => dbContext;
    public bool HasCollection(Type modelType) => dbContext.Model.FindEntityType(modelType) is not null;
}