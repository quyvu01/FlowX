using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IRemoveOneConditionResult<TModel, TResult> where TModel : class
{
    ICommandOneErrorDetailResult<TModel, TResult> WithCondition(
        Func<TModel, OneOf<None, Error>> condition);

    ICommandOneErrorDetailResult<TModel, TResult> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}

public interface IRemoveOneConditionVoid<TModel> where TModel : class
{
    ICommandOneErrorDetailVoid<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);

    ICommandOneErrorDetailVoid<TModel> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}