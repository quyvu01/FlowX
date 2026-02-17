using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public class QueryPipelineFlow : IStartQueryPipeline
{
    internal readonly List<IQueryPipelineStepEntry> StepEntries = [];
    internal Func<Task> BeforeExecutionFuncValue;
    internal Func<Task> AfterExecutionFuncValue;
    internal object ResultFuncAsyncValue;

    // === IStartQueryPipeline ===

    public IQueryPipelineOneStep<TModel> QueryOne<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
    {
        var step = new QueryOnePipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new QueryPipelineOneConfigurator<TModel, object>(this, step);
    }

    public IQueryPipelineManyStep<TModel> QueryMany<TModel>(
        Expression<Func<TModel, bool>> filter) where TModel : class
    {
        var step = new QueryManyPipelineStep<TModel, object> { Filter = filter };
        StepEntries.Add(step);
        return new QueryPipelineManyConfigurator<TModel, object>(this, step);
    }
}
