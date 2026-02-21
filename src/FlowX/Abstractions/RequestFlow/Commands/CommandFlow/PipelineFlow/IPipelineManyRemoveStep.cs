using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineManyRemoveStep<TModel> : IPipelineNextable<List<TModel>> where TModel : class
{
    IPipelineManyRemoveStep<TModel> WithCondition(Func<List<TModel>, OneOf<None, Error>> condition);
    IPipelineManyRemoveStep<TModel> WithCondition(Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync);
}