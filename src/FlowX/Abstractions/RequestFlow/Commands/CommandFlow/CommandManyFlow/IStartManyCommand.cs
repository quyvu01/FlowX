using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IStartManyCommandResult<TModel, TResult> where TModel : class
{
    ICreateManyConditionResult<TModel, TResult> CreateMany(Func<Task<IEnumerable<TModel>>> modelsAsync);
    ICreateManyConditionResult<TModel, TResult> CreateMany(Func<IEnumerable<TModel>> modelsFunc);
    ICreateManyConditionResult<TModel, TResult> CreateMany(IEnumerable<TModel> models);
    IUpdateManySpecialActionResult<TModel, TResult> UpdateMany(Expression<Func<TModel, bool>> filter);
    IRemoveManySpecialActionResult<TModel, TResult> RemoveMany(Expression<Func<TModel, bool>> filter);
}

public interface IStartManyCommandVoid<TModel> where TModel : class
{
    ICreateManyConditionVoid<TModel> CreateMany(Func<Task<IEnumerable<TModel>>> modelsAsync);
    ICreateManyConditionVoid<TModel> CreateMany(Func<IEnumerable<TModel>> modelsFunc);
    ICreateManyConditionVoid<TModel> CreateMany(IEnumerable<TModel> models);
    IUpdateManySpecialActionVoid<TModel> UpdateMany(Expression<Func<TModel, bool>> filter);
    IRemoveManySpecialActionVoid<TModel> RemoveMany(Expression<Func<TModel, bool>> filter);
}