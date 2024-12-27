using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IRemoveOneConditionResult<TModel, TResult> where TModel : class
{
    ICommandOneErrorDetailResult<TModel, TResult> WithCondition(
        System.Func<TModel, OneOf<None, ErrorDetail>> condition);

    ICommandOneErrorDetailResult<TModel, TResult> WithCondition(
        System.Func<TModel, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}

public interface IRemoveOneConditionVoid<TModel> where TModel : class
{
    ICommandOneErrorDetailVoid<TModel> WithCondition(System.Func<TModel, OneOf<None, ErrorDetail>> condition);

    ICommandOneErrorDetailVoid<TModel> WithCondition(
        System.Func<TModel, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}