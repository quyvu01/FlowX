using FlowX.Abstractions;
using FlowX.Errors;
using FlowX.Structs;
using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Repositories;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;

    protected EfUnitOfWork(DbContext dbContext) => _dbContext = dbContext;

    public async Task<OneOf<None, ErrorDetail>> SaveChangesAsync(CancellationToken token = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(token);
            return None.Value;
        }
        catch (Exception e)
        {
            var error = new ErrorDetail { Messages = [e.Message] };
            return OneOf<None, ErrorDetail>.FromT1(error);
        }
    }
}