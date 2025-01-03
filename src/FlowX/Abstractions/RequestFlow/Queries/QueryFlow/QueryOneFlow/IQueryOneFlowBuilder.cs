using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public interface IQueryOneFlowBuilder<TModel, out TResponse> where TModel : class where TResponse : class
{
    QuerySpecialActionType QuerySpecialActionType { get; }
    Expression<Func<TModel, bool>> Filter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> SpecialAction { get; }
    Func<IQueryable<TModel>, IQueryable<TResponse>> SpecialActionToResponse { get; }
    Func<TModel, TResponse> MapFunc { get; }
    Error Error { get; }
}