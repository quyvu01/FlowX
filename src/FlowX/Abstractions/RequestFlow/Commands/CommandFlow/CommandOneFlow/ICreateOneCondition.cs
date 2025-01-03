using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICreateOneConditionResult<TModel, TResult> where TModel : class
{
    ISaveChangesOneErrorDetailResult<TModel, TResult>
        WithCondition(Func<TModel, OneOf<None, Error>> condition);

    ISaveChangesOneErrorDetailResult<TModel, TResult> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}

public interface ICreateOneConditionVoid<TModel> where TModel : class
{
    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(Func<TModel, OneOf<None, Error>> condition);

    ISaveChangesOneErrorDetailVoid<TModel> WithCondition(
        Func<TModel, Task<OneOf<None, Error>>> conditionAsync);
}