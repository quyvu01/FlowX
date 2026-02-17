using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

public interface IQueryPipelineOneStep<TModel> : IQueryPipelineNextable<TModel> where TModel : class
{
    IQueryPipelineOneStep<TModel> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction);

    IQueryPipelineOneStep<TModel> WithErrorIfNull([NotNull] Error error);
}
