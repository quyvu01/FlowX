using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.CountingFlow;

public interface ICountingFlowBuilder<TModel> where TModel : class
{
    Expression<Func<TModel, bool>> Filter { get; }
}