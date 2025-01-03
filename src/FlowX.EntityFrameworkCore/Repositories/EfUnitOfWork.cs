using FlowX.Abstractions;
using FlowX.Errors;
using FlowX.Structs;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Repositories;

public class EfUnitOfWork(DbContext dbContext) : IUnitOfWork
{
    public async Task<OneOf<None, Error>> SaveChangesAsync(CancellationToken token = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(token);
            return None.Value;
        }
        catch (Exception e)
        {
            var error = new Error { Messages = [e.Message] };
            return OneOf<None, Error>.FromT1(error);
        }
    }
}