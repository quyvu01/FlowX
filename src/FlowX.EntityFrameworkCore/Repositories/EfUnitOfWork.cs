using FlowX.Abstractions;
using FlowX.Errors;
using FlowX.Structs;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Repositories;

public class EfUnitOfWork(DbContext dbContext) : IUnitOfWork
{
    public async Task<OneOf<None, ErrorDetail>> SaveChangesAsync(CancellationToken token = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(token);
            return None.Value;
        }
        catch (Exception e)
        {
            var error = new ErrorDetail { Messages = [e.Message] };
            return OneOf<None, ErrorDetail>.FromT1(error);
        }
    }
}