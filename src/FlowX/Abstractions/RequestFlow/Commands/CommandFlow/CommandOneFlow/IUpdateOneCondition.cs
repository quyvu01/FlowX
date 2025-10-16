using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IUpdateOneConditionResult<TModel, TResult> where TModel : class
{
    IUpdateOneModifyResult<TModel, TResult> WithCondition(Func<TModel, None> condition);
    IUpdateOneModifyResult<TModel, TResult> WithCondition(Func<TModel, Error> condition);
    IUpdateOneModifyResult<TModel, TResult> WithCondition(Func<TModel, Task<None>> conditionAsync);
    IUpdateOneModifyResult<TModel, TResult> WithCondition(Func<TModel, Task<Error>> conditionAsync);
}

public interface IUpdateOneConditionVoid<TModel> where TModel : class
{
    IUpdateOneModifyVoid<TModel> WithCondition(Func<TModel, None> condition);
    IUpdateOneModifyVoid<TModel> WithCondition(Func<TModel, Error> condition);
    IUpdateOneModifyVoid<TModel> WithCondition(Func<TModel, Task<None>> conditionAsync);
    IUpdateOneModifyVoid<TModel> WithCondition(Func<TModel, Task<Error>> conditionAsync);
}