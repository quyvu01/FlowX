using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Commands.PipelineFlow;

public interface IPipelineContinuation<out TPrev>
{
    // One transitions
    IPipelineOneCreateStep<TNext> ThenCreateOne<TNext>(
        Func<TPrev, TNext> factory) where TNext : class;

    IPipelineOneCreateStep<TNext> ThenCreateOne<TNext>(
        Func<TPrev, Task<TNext>> factoryAsync) where TNext : class;

    IPipelineOneUpdateStep<TNext> ThenUpdateOne<TNext>(
        Func<TPrev, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;

    IPipelineOneRemoveStep<TNext> ThenRemoveOne<TNext>(
        Func<TPrev, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;

    // Many transitions
    IPipelineManyCreateStep<TNext> ThenCreateMany<TNext>(
        Func<TPrev, IEnumerable<TNext>> factory) where TNext : class;

    IPipelineManyCreateStep<TNext> ThenCreateMany<TNext>(
        Func<TPrev, Task<IEnumerable<TNext>>> factoryAsync) where TNext : class;

    IPipelineManyUpdateStep<TNext> ThenUpdateMany<TNext>(
        Func<TPrev, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;

    IPipelineManyRemoveStep<TNext> ThenRemoveMany<TNext>(
        Func<TPrev, Expression<Func<TNext, bool>>> filterFactory) where TNext : class;
}