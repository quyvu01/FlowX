using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IUpdateManyConditionResult<TModel, TResult> where TModel : class
{
    IUpdateManyModifyResult<TModel, TResult> WithCondition(
        System.Func<List<TModel>, OneOf<None, ErrorDetail>> condition);

    IUpdateManyModifyResult<TModel, TResult> WithCondition(
        System.Func<List<TModel>, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}

public interface IUpdateManyConditionVoid<TModel> where TModel : class
{
    IUpdateManyModifyVoid<TModel> WithCondition(System.Func<List<TModel>, OneOf<None, ErrorDetail>> condition);

    IUpdateManyModifyVoid<TModel> WithCondition(
        System.Func<List<TModel>, Task<OneOf<None, ErrorDetail>>> conditionAsync);
}