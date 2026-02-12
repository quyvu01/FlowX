using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IStartPipeline
{
    IPipelineCreateStep<TModel> CreateOne<TModel>(TModel model) where TModel : class;
    IPipelineCreateStep<TModel> CreateOne<TModel>(Func<TModel> modelFunc) where TModel : class;
    IPipelineCreateStep<TModel> CreateOne<TModel>(Func<Task<TModel>> modelFuncAsync) where TModel : class;
    IPipelineUpdateStep<TModel> UpdateOne<TModel>(Expression<Func<TModel, bool>> filter) where TModel : class;
    IPipelineRemoveStep<TModel> RemoveOne<TModel>(Expression<Func<TModel, bool>> filter) where TModel : class;
}
