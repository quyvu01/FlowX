using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineNextable<out TModel> where TModel : class
{
    IPipelineAfterDone<TModel> Done();

    IPipelineCreateStep<TNext> ThenCreateOne<TNext>(
        Func<TModel, TNext> factory) where TNext : class;

    IPipelineCreateStep<TNext> ThenCreateOne<TNext>(
        Func<TModel, Task<TNext>> factoryAsync) where TNext : class;

    IPipelineUpdateStep<TNext> ThenUpdateOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;

    IPipelineRemoveStep<TNext> ThenRemoveOne<TNext>(
        Func<TModel, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;

    IPipelineTerminal WithErrorIfSaveChange([NotNull] Error error);
}
