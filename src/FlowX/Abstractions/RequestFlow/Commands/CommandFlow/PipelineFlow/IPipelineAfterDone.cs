using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IPipelineAfterDone<out TPrev> where TPrev : class
{
    IPipelineCreateStep<TModel> ThenCreateOne<TModel>(
        Func<TPrev, TModel> factory) where TModel : class;

    IPipelineCreateStep<TModel> ThenCreateOne<TModel>(
        Func<TPrev, Task<TModel>> factoryAsync) where TModel : class;

    IPipelineUpdateStep<TModel> ThenUpdateOne<TModel>(
        Func<TPrev, Expression<Func<TModel, bool>>> filterFactory) where TModel : class;

    IPipelineRemoveStep<TModel> ThenRemoveOne<TModel>(
        Func<TPrev, Expression<Func<TModel, bool>>> filterFactory) where TModel : class;

    IPipelineTerminal WithErrorIfSaveChange([NotNull] Error error);

    IPipelineResultTerminal<TResult> WithResultIfSucceed<TResult>(
        [NotNull] Func<TPrev, TResult> resultFunc);

    IPipelineResultTerminal<TResult> WithResultIfSucceed<TResult>(
        [NotNull] Func<TPrev, Task<TResult>> resultFuncAsync);
}
