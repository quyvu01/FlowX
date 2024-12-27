using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICreateOneConditionResult<TModel, TResult> where TModel : class
{
    ISaveChangesOneErrorDetailResult<TModel, TResult>
        WithCondition(System.Func<TModel, OneOf<None, ErrorDetail>> condition);

    ISaveChangesOneErrorDetailResult<TModel, TResult> WithCondition(
        System.Func<TModel, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}

public interface ICreateOneConditionVoid<TModel> where TModel : class
{
    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(System.Func<TModel, OneOf<None, ErrorDetail>> condition);

    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(
        System.Func<TModel, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}