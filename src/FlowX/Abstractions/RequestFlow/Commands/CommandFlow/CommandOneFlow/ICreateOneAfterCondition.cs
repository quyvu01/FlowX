using System.Diagnostics.CodeAnalysis;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICreateOneAfterConditionResult<TModel, TResult> where TModel : class
{
    ICreateOneAfterConditionResult<TModel, TResult> WithCondition(
        Func<TModel, OneOf<None, Error>> condition);

    ICreateOneAfterConditionResult<TModel, TResult> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);

    ISaveChangesOneErrorDetailResult<TModel, TResult> WithModify(
        [NotNull] Action<TModel> modifyAction);

    ISaveChangesOneErrorDetailResult<TModel, TResult> WithModify(
        [NotNull] Func<TModel, Task> modifyActionAsync);

    ISaveChangesOneErrorDetailResult<TModel, TResult> WithBeforeExecution(
        [NotNull] Action<TModel> action);

    ISaveChangesOneErrorDetailResult<TModel, TResult> WithBeforeExecution(
        [NotNull] Func<TModel, Task> actionAsync);

    IAfterSaveChangeResult<TModel, TResult> WithErrorIfSaveChange([NotNull] Error error);
}

public interface ICreateOneAfterConditionVoid<TModel> where TModel : class
{
    ICreateOneAfterConditionVoid<TModel> WithCondition(
        Func<TModel, OneOf<None, Error>> condition);

    ICreateOneAfterConditionVoid<TModel> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);

    ISaveChangesOneErrorDetailVoid<TModel> WithModify(
        [NotNull] Action<TModel> modifyAction);

    ISaveChangesOneErrorDetailVoid<TModel> WithModify(
        [NotNull] Func<TModel, Task> modifyActionAsync);

    ISaveChangesOneErrorDetailVoid<TModel> WithBeforeExecution(
        [NotNull] Action<TModel> action);

    ISaveChangesOneErrorDetailVoid<TModel> WithBeforeExecution(
        [NotNull] Func<TModel, Task> actionAsync);

    IAfterSaveChangeVoid<TModel> WithErrorIfSaveChange([NotNull] Error error);
}
