using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ISaveChangesOneErrorDetailResult<TModel, TResult> where TModel : class
{
    ISaveChangesOneErrorDetailResult<TModel, TResult> WithBeforeExecution(
        [NotNull] Action<TModel> action);

    ISaveChangesOneErrorDetailResult<TModel, TResult> WithBeforeExecution(
        [NotNull] Func<TModel, Task> actionAsync);

    IAfterSaveChangeResult<TModel, TResult> WithErrorIfSaveChange([NotNull] Error error);
}

public interface ISaveChangesOneErrorDetailVoid<TModel> where TModel : class
{
    ISaveChangesOneErrorDetailVoid<TModel> WithBeforeExecution(
        [NotNull] Action<TModel> action);

    ISaveChangesOneErrorDetailVoid<TModel> WithBeforeExecution(
        [NotNull] Func<TModel, Task> actionAsync);

    IAfterSaveChangeVoid<TModel> WithErrorIfSaveChange([NotNull] Error error);
}
