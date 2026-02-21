using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineManyUpdateStep<TModel> : IPipelineNextable<List<TModel>> where TModel : class
{
    IPipelineManyUpdateStep<TModel> WithCondition(Func<List<TModel>, OneOf<None, Error>> condition);
    IPipelineManyUpdateStep<TModel> WithCondition(Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync);
    IPipelineManyUpdateStep<TModel> WithModify([NotNull] Action<List<TModel>> modifyAction);
    IPipelineManyUpdateStep<TModel> WithModify([NotNull] Func<List<TModel>, Task> modifyActionAsync);
}