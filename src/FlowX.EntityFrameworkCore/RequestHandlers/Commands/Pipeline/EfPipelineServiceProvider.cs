using System.Linq.Expressions;
using FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;
using FlowX.EntityFrameworkCore.Abstractions;

namespace FlowX.EntityFrameworkCore.RequestHandlers.Commands.Pipeline;

internal sealed class EfPipelineServiceProvider(IUnitOfWork unitOfWork) : IPipelineServiceProvider
{
    public async Task<TModel> CreateOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class
    {
        var repository = unitOfWork.RepositoryOf<TModel>();
        return await repository.CreateOneAsync(model, ct);
    }

    public async Task<TModel> GetFirstByConditionAsync<TModel>(
        Expression<Func<TModel, bool>> filter, CancellationToken ct) where TModel : class
    {
        var repository = unitOfWork.RepositoryOf<TModel>();
        return await repository.GetFirstByConditionAsync(filter, token: ct);
    }

    public async Task RemoveOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class
    {
        var repository = unitOfWork.RepositoryOf<TModel>();
        await repository.RemoveOneAsync(model, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => unitOfWork.SaveChangesAsync(ct);
}