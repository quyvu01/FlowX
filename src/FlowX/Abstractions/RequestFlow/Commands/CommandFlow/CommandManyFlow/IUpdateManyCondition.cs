using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IUpdateManyConditionResult<TModel, TResult> where TModel : class
{
    IUpdateManyModifyResult<TModel, TResult> WithCondition(Func<List<TModel>, None> condition);
    IUpdateManyModifyResult<TModel, TResult> WithCondition(Func<List<TModel>, Error> condition);
    IUpdateManyModifyResult<TModel, TResult> WithCondition(Func<List<TModel>, Task<None>> conditionAsync);
    IUpdateManyModifyResult<TModel, TResult> WithCondition(Func<List<TModel>, Task<Error>> conditionAsync);
}

public interface IUpdateManyConditionVoid<TModel> where TModel : class
{
    IUpdateManyModifyVoid<TModel> WithCondition(Func<List<TModel>, None> condition);
    IUpdateManyModifyVoid<TModel> WithCondition(Func<List<TModel>, Error> condition);
    IUpdateManyModifyVoid<TModel> WithCondition(Func<List<TModel>, Task<None>> conditionAsync);
    IUpdateManyModifyVoid<TModel> WithCondition(Func<List<TModel>, Task<Error>> conditionAsync);
}