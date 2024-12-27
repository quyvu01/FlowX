using System.Linq.Expressions;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListFlowBuilder<TModel, out TResponse> where TModel : class
{
    QuerySpecialActionType QuerySpecialActionType { get; }
    Expression<Func<TModel, bool>> Filter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> SpecialActionToModel { get; }
    Func<IQueryable<TModel>, IQueryable<TResponse>> SpecialActionToResponse { get; }
    Expression<Func<TModel, object>> SortFieldNameWhenRequestEmpty { get; }
    SortedDirection SortedDirectionWhenRequestEmpty { get; }
}