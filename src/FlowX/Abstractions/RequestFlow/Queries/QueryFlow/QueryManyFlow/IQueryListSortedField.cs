namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;

public interface IQueryListSortedField<TModel, out TResponse> where TModel : class
{
    IQueryListFlowBuilder<TModel, TResponse> WithDefaultSortFields(ExpressionOrder<TModel> expressionOrder);
}