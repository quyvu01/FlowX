using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IAfterSaveChangeResult<TModel, TResult> : ISaveChangesOneSucceed<TModel, TResult>
    where TModel : class
{
    ISaveChangesOneSucceed<TModel, TResult> WithAfterExecution([NotNull] Action<TModel> action);
    ISaveChangesOneSucceed<TModel, TResult> WithAfterExecution([NotNull] Func<TModel, Task> actionAsync);
}

public interface IAfterSaveChangeVoid<TModel> : ICommandOneFlowBuilderVoid<TModel>
    where TModel : class
{
    ICommandOneFlowBuilderVoid<TModel> WithAfterExecution([NotNull] Action<TModel> action);
    ICommandOneFlowBuilderVoid<TModel> WithAfterExecution([NotNull] Func<TModel, Task> actionAsync);
}
