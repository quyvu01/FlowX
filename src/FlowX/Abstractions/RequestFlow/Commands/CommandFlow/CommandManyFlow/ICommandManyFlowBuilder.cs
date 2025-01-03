using System.Linq.Expressions;
using FlowX.ApplicationModels;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface ICommandManyFlowBuilderResult<TModel, out TResult> where TModel : class
{
    CommandTypeMany CommandTypeMany { get; }
    Func<Task<List<TModel>>> ModelsCreateFunc { get; }
    Func<List<TModel>, Task<OneOf<None, Error>>> CommandManyCondition { get; }
    Expression<Func<TModel, bool>> CommandFilter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; }
    Func<List<TModel>, Task> UpdateManyFunc { get; }
    Error NullError { get; }
    Error SaveChangesError { get; }
    Func<List<TModel>, TResult> ResultFunc { get; }
}

public interface ICommandManyFlowBuilderVoid<TModel> where TModel : class
{
    CommandTypeMany CommandTypeMany { get; }
    Func<Task<List<TModel>>> ModelsCreateFunc { get; }
    Func<List<TModel>, Task<OneOf<None, Error>>> CommandManyCondition { get; }
    Expression<Func<TModel, bool>> CommandFilter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; }
    Func<List<TModel>, Task> UpdateManyFunc { get; }
    Error NullError { get; }
    Error SaveChangesError { get; }
}