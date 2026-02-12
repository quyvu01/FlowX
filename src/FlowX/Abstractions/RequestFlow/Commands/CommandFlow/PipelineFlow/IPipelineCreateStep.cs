using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineCreateStep<out TModel> : IPipelineNextable<TModel> where TModel : class
{
    IPipelineCreateStep<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);
    IPipelineCreateStep<TModel> WithCondition(Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
    IPipelineCreateStep<TModel> WithModify([NotNull] Action<TModel> modifyAction);
    IPipelineCreateStep<TModel> WithModify([NotNull] Func<TModel, Task> modifyActionAsync);
}
