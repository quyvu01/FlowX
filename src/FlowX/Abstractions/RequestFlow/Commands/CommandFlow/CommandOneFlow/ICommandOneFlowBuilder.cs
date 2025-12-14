using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICommandOneFlowBuilderResult<TModel, out TResult> : ICommandOneFlowBuilderVoid<TModel>
    where TModel : class
{
    Func<TModel, TResult> ResultFunc { get; }
}

public interface ICommandOneFlowBuilderVoid<TModel> where TModel : class
{
    CommandTypeOne CommandTypeOne { get; }
    Func<Task<TModel>> ModelCreateFunc { get; }
    Func<TModel, Task<OneOf<None, Error>>> ConditionAsync { get; }
    Expression<Func<TModel, bool>> CommandFilter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; }
    Func<TModel, Task> UpdateOneFunc { get; }
    Error NullError { get; }
    Error SaveChangesError { get; }
}