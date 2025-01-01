using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions;

public interface IUnitOfWork
{
    Task<OneOf<None, ErrorDetail>> SaveChangesAsync(CancellationToken token = default);
}