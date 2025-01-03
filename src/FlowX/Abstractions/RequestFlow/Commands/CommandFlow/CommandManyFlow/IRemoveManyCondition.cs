using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IRemoveManyConditionResult<TModel, TResult> where TModel : class
{
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, OneOf<None, Error>> condition);

    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync);
}

public interface IRemoveManyConditionVoid<TModel> where TModel : class
{
    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, OneOf<None, Error>> condition);

    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync);
}