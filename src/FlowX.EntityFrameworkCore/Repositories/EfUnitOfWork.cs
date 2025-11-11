using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Delegates;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.EntityFrameworkCore.Repositories;

public class EfUnitOfWork(IServiceProvider serviceProvider) : IUnitOfWork
{
    public IRepository<TModel> RepositoryOf<TModel>() where TModel : class =>
        serviceProvider.GetService<EfRepository<TModel>>();

    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        var dbContexts = serviceProvider.GetRequiredService<GetDbContexts>().Invoke();
        await Task.WhenAll(dbContexts.Select(a => a.SaveChangesAsync(token)));
    }
}