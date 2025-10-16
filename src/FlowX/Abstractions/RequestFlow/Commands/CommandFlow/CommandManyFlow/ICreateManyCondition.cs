using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface ICreateManyConditionResult<TModel, TResult> where TModel : class
{
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, None> condition);
    
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, Error> condition);

    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, Task<None>> conditionAsync);
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, Task<Error>> conditionAsync);
}

public interface ICreateManyConditionVoid<TModel> where TModel : class
{
    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, None> condition);
    
    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, Error> condition);

    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, Task<None>> conditionAsync);
    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, Task<Error>> conditionAsync);
}