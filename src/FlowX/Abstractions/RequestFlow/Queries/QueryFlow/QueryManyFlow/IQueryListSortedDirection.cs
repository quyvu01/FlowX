using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListSortedDirection<TModel, out TResponse> where TModel : class
{
    IQueryListFlowBuilder<TModel, TResponse> WithSortedDirectionWhenNotSet(SortedDirection direction);
}