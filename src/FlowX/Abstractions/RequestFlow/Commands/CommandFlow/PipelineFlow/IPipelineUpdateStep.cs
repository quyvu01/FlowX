using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineUpdateStep<out TModel> : IPipelineNextable<TModel> where TModel : class
{
    IPipelineUpdateStep<TModel> WithErrorIfNull([NotNull] Error error);
    IPipelineUpdateStep<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);
    IPipelineUpdateStep<TModel> WithCondition(Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
    IPipelineUpdateStep<TModel> WithModify([NotNull] Action<TModel> modifyAction);
    IPipelineUpdateStep<TModel> WithModify([NotNull] Func<TModel, Task> modifyActionAsync);
}
