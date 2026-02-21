using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineOneRemoveStep<TModel> : IPipelineNextable<TModel> where TModel : class
{
    IPipelineOneRemoveStep<TModel> WithErrorIfNull([NotNull] Error error);
    IPipelineOneRemoveStep<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);
    IPipelineOneRemoveStep<TModel> WithCondition(Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}
