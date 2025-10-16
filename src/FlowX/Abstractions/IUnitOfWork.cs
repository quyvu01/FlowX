using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken token = default);
}