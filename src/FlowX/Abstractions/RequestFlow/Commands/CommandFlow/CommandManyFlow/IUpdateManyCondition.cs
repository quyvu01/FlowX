using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IUpdateManyConditionResult<TModel, TResult> where TModel : class
{
    IUpdateManyModifyResult<TModel, TResult> WithCondition(
        Func<List<TModel>, OneOf<None, Error>> condition);

    IUpdateManyModifyResult<TModel, TResult> WithCondition(
        Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync);
}

public interface IUpdateManyConditionVoid<TModel> where TModel : class
{
    IUpdateManyModifyVoid<TModel> WithCondition(Func<List<TModel>, OneOf<None, Error>> condition);

    IUpdateManyModifyVoid<TModel> WithCondition(
        Func<List<TModel>, Task<OneOf<None, Error>>> conditionAsync);
}