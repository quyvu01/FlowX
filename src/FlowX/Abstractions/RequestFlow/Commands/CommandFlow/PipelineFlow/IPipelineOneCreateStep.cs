using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineOneCreateStep<TModel> : IPipelineNextable<TModel> where TModel : class
{
    IPipelineOneCreateStep<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);
    IPipelineOneCreateStep<TModel> WithCondition(Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
    IPipelineOneCreateStep<TModel> WithModify([NotNull] Action<TModel> modifyAction);
    IPipelineOneCreateStep<TModel> WithModify([NotNull] Func<TModel, Task> modifyActionAsync);
}
