using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineNextable<TPrev>
{
    // Filter-based transitions
    IQueryPipelineOneStep<TNext> ThenQueryOne<TNext>(
        Func<TPrev, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;

    IQueryPipelineManyStep<TNext> ThenQueryMany<TNext>(
        Func<TPrev, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;

    // Queryable-based transitions
    IQueryPipelineOneStep<TNext> ThenQueryOneFromQueryable<TNext>(
        Func<TPrev, IQueryable<TNext>, IQueryable<TNext>> queryableFactory) where TNext : class;

    IQueryPipelineManyStep<TNext> ThenQueryManyFromQueryable<TNext>(
        Func<TPrev, IQueryable<TNext>, IQueryable<TNext>> queryableFactory) where TNext : class;

    // Terminal
    IQueryPipelineResultTerminal<TResult> WithResult<TResult>(
        [NotNull] Func<TPrev, TResult> resultFunc);

    IQueryPipelineResultTerminal<TResult> WithResult<TResult>(
        [NotNull] Func<TPrev, Task<TResult>> resultFuncAsync);
}
