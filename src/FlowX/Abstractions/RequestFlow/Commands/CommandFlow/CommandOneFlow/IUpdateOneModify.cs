using System.Diagnostics.CodeAnalysis;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.CommandOneFlow;

public interface IUpdateOneModifyResult<TModel, TResult> where TModel : class
{
    ICommandOneErrorDetailResult<TModel, TResult> WithModify([NotNull] Func<TModel, Task> updateFuncAsync);
    ICommandOneErrorDetailResult<TModel, TResult> WithModify([NotNull] Action<TModel> updateFunc);
}

public interface IUpdateOneModifyVoid<TModel> where TModel : class
{
    ICommandOneErrorDetailVoid<TModel> WithModify([NotNull] Func<TModel, Task> updateFuncAsync);
    ICommandOneErrorDetailVoid<TModel> WithModify([NotNull] Action<TModel> updateFunc);
}