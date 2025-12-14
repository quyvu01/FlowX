using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandManyFlow;

public interface IUpdateManyModifyResult<TModel, TResult> where TModel : class
{
    ISaveChangesManyErrorDetailResult<TModel, TResult> WithModify(
        [NotNull] Func<IReadOnlyCollection<TModel>, Task> updateFuncAsync);

    ISaveChangesManyErrorDetailResult<TModel, TResult> WithModify(
        [NotNull] Action<IReadOnlyCollection<TModel>> updateFunc);
}

public interface IUpdateManyModifyVoid<TModel> where TModel : class
{
    ISaveChangesManyErrorDetailVoid<TModel> WithModify(
        [NotNull] Func<IReadOnlyCollection<TModel>, Task> updateFuncAsync);

    ISaveChangesManyErrorDetailVoid<TModel> WithModify(
        [NotNull] Action<IReadOnlyCollection<TModel>> updateFunc);
}