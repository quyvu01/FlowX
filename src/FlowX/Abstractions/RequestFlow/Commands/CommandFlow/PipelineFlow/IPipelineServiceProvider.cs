using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineServiceProvider
{
    Task<TModel> CreateOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class;

    Task<TModel> GetFirstByConditionAsync<TModel>(Expression<Func<TModel, bool>> filter,
        CancellationToken ct) where TModel : class;

    Task RemoveOneAsync<TModel>(TModel model, CancellationToken ct) where TModel : class;
    Task SaveChangesAsync(CancellationToken ct);
}
