using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IUpdateOneAfterConditionResult<TModel, TResult> : IUpdateOneModifyResult<TModel, TResult>
    where TModel : class
{
    IUpdateOneAfterConditionResult<TModel, TResult> WithCondition(
        Func<TModel, OneOf<None, Error>> condition);

    IUpdateOneAfterConditionResult<TModel, TResult> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}

public interface IUpdateOneAfterConditionVoid<TModel> : IUpdateOneModifyVoid<TModel>
    where TModel : class
{
    IUpdateOneAfterConditionVoid<TModel> WithCondition(
        Func<TModel, OneOf<None, Error>> condition);

    IUpdateOneAfterConditionVoid<TModel> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}
