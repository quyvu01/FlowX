namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface ISaveChangesManySucceedResult<TModel, TResult> where TModel : class
{
    ICommandManyFlowBuilderResult<TModel, TResult> WithResultIfSucceed(Func<List<TModel>, TResult> resultFunc);
}