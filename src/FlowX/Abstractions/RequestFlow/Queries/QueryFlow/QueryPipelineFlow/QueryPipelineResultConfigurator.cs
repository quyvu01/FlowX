namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryPipelineFlow;

internal sealed class QueryPipelineResultConfigurator<TResult> :
    IQueryPipelineResultTerminal<TResult>
{
    private readonly QueryPipelineFlow _pipeline;

    internal QueryPipelineResultConfigurator(QueryPipelineFlow pipeline)
    {
        _pipeline = pipeline;
    }

    // ===== IQueryPipelineFlowBuilder<TResult> =====

    IReadOnlyList<IQueryPipelineStepEntry> IQueryPipelineFlowBuilder<TResult>.Steps => _pipeline.StepEntries;

    Func<object, Task<TResult>> IQueryPipelineFlowBuilder<TResult>.ResultFuncAsync =>
        (Func<object, Task<TResult>>)_pipeline.ResultFuncAsyncValue;

    Func<Task> IQueryPipelineFlowBuilder<TResult>.BeforeExecutionFunc => _pipeline.BeforeExecutionFuncValue;
    Func<Task> IQueryPipelineFlowBuilder<TResult>.AfterExecutionFunc => _pipeline.AfterExecutionFuncValue;

    // ===== IQueryPipelineResultTerminal<TResult> =====

    IQueryPipelineResultTerminal<TResult> IQueryPipelineResultTerminal<TResult>.WithBeforeExecution(Action action)
    {
        _pipeline.BeforeExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryPipelineResultTerminal<TResult> IQueryPipelineResultTerminal<TResult>.WithBeforeExecution(
        Func<Task> actionAsync)
    {
        _pipeline.BeforeExecutionFuncValue = actionAsync;
        return this;
    }

    IQueryPipelineFlowBuilder<TResult> IQueryPipelineResultTerminal<TResult>.WithAfterExecution(Action action)
    {
        _pipeline.AfterExecutionFuncValue = () =>
        {
            action();
            return Task.CompletedTask;
        };
        return this;
    }

    IQueryPipelineFlowBuilder<TResult> IQueryPipelineResultTerminal<TResult>.WithAfterExecution(
        Func<Task> actionAsync)
    {
        _pipeline.AfterExecutionFuncValue = actionAsync;
        return this;
    }
}
