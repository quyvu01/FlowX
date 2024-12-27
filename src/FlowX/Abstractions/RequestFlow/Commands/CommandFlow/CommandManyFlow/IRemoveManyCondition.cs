using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IRemoveManyConditionResult<TModel, TResult> where TModel : class
{
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        System.Func<List<TModel>, OneOf<None, ErrorDetail>> condition);

    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        System.Func<List<TModel>, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}

public interface IRemoveManyConditionVoid<TModel> where TModel : class
{
    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        System.Func<List<TModel>, OneOf<None, ErrorDetail>> condition);

    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        System.Func<List<TModel>, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}