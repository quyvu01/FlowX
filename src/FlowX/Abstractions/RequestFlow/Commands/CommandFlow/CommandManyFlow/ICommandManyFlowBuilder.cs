using System.Linq.Expressions;
using FlowX.Errors;
using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface ICommandManyFlowBuilderResult<TModel, out TResult> :
    ICommandManyFlowBuilderVoid<TModel> where TModel : class
{
    Func<IReadOnlyCollection<TModel>, TResult> ResultFunc { get; }
}

public interface ICommandManyFlowBuilderVoid<TModel> where TModel : class
{
    CommandTypeMany CommandTypeMany { get; }
    Func<Task<IEnumerable<TModel>>> ModelsAsync { get; }
    Func<IReadOnlyCollection<TModel>, Task<OneOf<None, Error>>> ConditionAsync { get; }
    Expression<Func<TModel, bool>> CommandFilter { get; }
    Func<IQueryable<TModel>, IQueryable<TModel>> CommandSpecialAction { get; }
    Func<IReadOnlyCollection<TModel>, Task> UpdateManyFunc { get; }
    Error NullError { get; }
    Error SaveChangesError { get; }
}