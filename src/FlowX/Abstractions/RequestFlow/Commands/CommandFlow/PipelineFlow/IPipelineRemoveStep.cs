using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineRemoveStep<out TModel> : IPipelineNextable<TModel> where TModel : class
{
    IPipelineRemoveStep<TModel> WithErrorIfNull([NotNull] Error error);
    IPipelineRemoveStep<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);
    IPipelineRemoveStep<TModel> WithCondition(Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}
