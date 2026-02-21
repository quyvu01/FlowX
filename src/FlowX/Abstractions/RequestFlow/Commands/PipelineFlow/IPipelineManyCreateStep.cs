using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public interface IPipelineManyCreateStep<TModel> : IPipelineNextable<List<TModel>> where TModel : class
{
    IPipelineManyCreateStep<TModel> WithCondition(Func<List<TModel>, OneOf<None, Error>> condition);
    IPipelineManyCreateStep<TModel> WithCondition(Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync);
    IPipelineManyCreateStep<TModel> WithModify([NotNull] Action<TModel> modifyAction);
    IPipelineManyCreateStep<TModel> WithModify([NotNull] Func<TModel, Task> modifyActionAsync);
}
