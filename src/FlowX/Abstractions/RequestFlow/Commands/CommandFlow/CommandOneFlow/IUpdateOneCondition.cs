using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IUpdateOneConditionResult<TModel, TResult> where TModel : class
{
    IUpdateOneModifyResult<TModel, TResult> WithCondition(Func<TModel, OneOf<None, ErrorDetail>> condition);

    IUpdateOneModifyResult<TModel, TResult> WithCondition(
        Func<TModel, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}

public interface IUpdateOneConditionVoid<TModel> where TModel : class
{
    IUpdateOneModifyVoid<TModel> WithCondition(Func<TModel, OneOf<None, ErrorDetail>> condition);
    IUpdateOneModifyVoid<TModel> WithCondition(Func<TModel, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}