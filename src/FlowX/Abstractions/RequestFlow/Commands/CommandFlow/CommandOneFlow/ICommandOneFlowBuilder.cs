using System.Linq.Expressions;
using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface ICommandOneFlowBuilderResult<TModel, out TResult> where TModel : class
{
    CommandTypeOne CommandTypeOne { get; }
    Func<Task<TModel>> ModelCreateFunc { get; }
    Func<TModel, Task<OneOf<None, ErrorDetail>>> CommandOneCondition { get; }
    Expression<Func<TModel, bool>> CommandFilter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; }
    Func<TModel, Task> UpdateOneFunc { get; }
    ErrorDetail NullErrorDetail { get; }
    ErrorDetail SaveChangesErrorDetail { get; }
    Func<TModel, TResult> ResultFunc { get; }
}
public interface ICommandOneFlowBuilderVoid<TModel> where TModel : class
{
    CommandTypeOne CommandTypeOne { get; }
    Func<Task<TModel>> ModelCreateFunc { get; }
    Func<TModel, Task<OneOf<None, ErrorDetail>>> CommandOneCondition { get; }
    Expression<Func<TModel, bool>> CommandFilter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; }
    Func<TModel, Task> UpdateOneFunc { get; }
    ErrorDetail NullErrorDetail { get; }
    ErrorDetail SaveChangesErrorDetail { get; }
}