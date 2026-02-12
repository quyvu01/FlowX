using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICreateOneConditionResult<TModel, TResult> where TModel : class
{
    ICreateOneAfterConditionResult<TModel, TResult> WithCondition(
        Func<TModel, OneOf<None, Error>> condition);

    ICreateOneAfterConditionResult<TModel, TResult> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}

public interface ICreateOneConditionVoid<TModel> where TModel : class
{
    ICreateOneAfterConditionVoid<TModel> WithCondition(
        Func<TModel, OneOf<None, Error>> condition);

    ICreateOneAfterConditionVoid<TModel> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}