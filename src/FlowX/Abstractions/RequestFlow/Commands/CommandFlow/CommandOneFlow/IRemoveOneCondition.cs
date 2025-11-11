using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IRemoveOneConditionResult<TModel, TResult> where TModel : class
{
    ICommandOneErrorDetailResult<TModel, TResult> WithCondition( Func<TModel, None> condition);
    ICommandOneErrorDetailResult<TModel, TResult> WithCondition( Func<TModel, Error> condition);
    ICommandOneErrorDetailResult<TModel, TResult> WithCondition(Func<TModel, Task<None>> conditionAsync);
    ICommandOneErrorDetailResult<TModel, TResult> WithCondition(Func<TModel, Task<Error>> conditionAsync);
}

public interface IRemoveOneConditionVoid<TModel> where TModel : class
{
    ICommandOneErrorDetailVoid<TModel> WithCondition(Func<TModel, None> condition);
    ICommandOneErrorDetailVoid<TModel> WithCondition(Func<TModel, Error> condition);
    ICommandOneErrorDetailVoid<TModel> WithCondition(Func<TModel, Task<None>> conditionAsync);
    ICommandOneErrorDetailVoid<TModel> WithCondition(Func<TModel, Task<Error>> conditionAsync);
}