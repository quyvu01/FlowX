using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface ICreateManyConditionResult<TModel, TResult> where TModel : class
{
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, OneOf<None, ErrorDetail>> condition);

    ISaveChangesManyErrorDetailResult<TModel, TResult> WithCondition(
        Func<List<TModel>, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}

public interface ICreateManyConditionVoid<TModel> where TModel : class
{
    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, OneOf<None, ErrorDetail>> condition);

    ISaveChangesManyErrorDetailVoid<TModel> WithCondition(
        Func<List<TModel>, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}