namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ISaveChangesOneSucceed<TModel, TResult> where TModel : class
{
    ICommandOneFlowBuilderResult<TModel, TResult> WithResultIfSucceed(Func<TModel, TResult> resultFunc);
}