using FlowX.EntityFrameworkCore.Abstractions;

namespace FlowX.EntityFrameworkCore.SharedStates;

internal sealed record UnitOfWorkContext(IUnitOfWork UnitOfWork);

internal static class EfCoreSharedStates
{
    private static readonly AsyncLocal<UnitOfWorkContext> AsyncLocal = new();

    public static UnitOfWorkContext CreateContext(IUnitOfWork unitOfWork)
    {
        AsyncLocal.Value = new UnitOfWorkContext(unitOfWork);
        return AsyncLocal.Value;
    }

    public static IUnitOfWork GetUnitOfWork()
    {
        var result = AsyncLocal.Value?.UnitOfWork;
        return result;
    }
}