using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICreateOneConditionResult<TModel, TResult> where TModel : class
{
    ISaveChangesOneErrorDetailResult<TModel, TResult> WithCondition(Func<TModel, None> condition);
    ISaveChangesOneErrorDetailResult<TModel, TResult> WithCondition(Func<TModel, Error> condition);
    ISaveChangesOneErrorDetailResult<TModel, TResult> WithCondition(Func<TModel, Task<None>> conditionAsync);
    ISaveChangesOneErrorDetailResult<TModel, TResult> WithCondition(Func<TModel, Task<Error>> conditionAsync);
}

public interface ICreateOneConditionVoid<TModel> where TModel : class
{
    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(Func<TModel, None> condition);
    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(Func<TModel, Error> condition);
    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(Func<TModel, Task<None>> conditionAsync);
    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(Func<TModel, Task<Error>> conditionAsync);
}