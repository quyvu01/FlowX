using Microsoft.EntityFrameworkCore;

namespace FlowX.EntityFrameworkCore.Abstractions;

internal interface IDbContext
{
    DbContext DbContext { get; }
    bool HasCollection(Type modelType);
}