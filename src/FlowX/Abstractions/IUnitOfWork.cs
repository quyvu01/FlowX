using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions;

public interface IUnitOfWork
{
    Task<OneOf<None, Error>> SaveChangesAsync(CancellationToken token = default);
}