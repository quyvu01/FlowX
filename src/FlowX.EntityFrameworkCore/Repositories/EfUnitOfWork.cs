using FlowX.Abstractions;
using FlowX.Errors;
using FlowX.Structs;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Repositories;

public class EfUnitOfWork<TDbContext>(TDbContext dbContext) : IUnitOfWork where TDbContext : DbContext
{
    public async Task<int> SaveChangesAsync(CancellationToken token = default) =>
        await dbContext.SaveChangesAsync(token);
}