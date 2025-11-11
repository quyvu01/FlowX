namespace FlowX.EntityFrameworkCore.Abstractions;

public interface IUnitOfWork
{
    IRepository<TModel> RepositoryOf<TModel>() where TModel : class;
    Task SaveChangesAsync(CancellationToken token = default);
}