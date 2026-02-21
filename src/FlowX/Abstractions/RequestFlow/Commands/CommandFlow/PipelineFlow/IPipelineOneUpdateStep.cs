using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineOneUpdateStep<TModel> : IPipelineNextable<TModel> where TModel : class
{
    IPipelineOneUpdateStep<TModel> WithErrorIfNull([NotNull] Error error);
    IPipelineOneUpdateStep<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);
    IPipelineOneUpdateStep<TModel> WithCondition(Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
    IPipelineOneUpdateStep<TModel> WithModify([NotNull] Action<TModel> modifyAction);
    IPipelineOneUpdateStep<TModel> WithModify([NotNull] Func<TModel, Task> modifyActionAsync);
}
