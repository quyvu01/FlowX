using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IStartQueryPipeline
{
    IQueryPipelineOneStep<TModel> QueryOne<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class;

    IQueryPipelineManyStep<TModel> QueryMany<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class;
}
