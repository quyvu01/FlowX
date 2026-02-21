using System.Linq.Expressions;

namespace FlowX.Abstractions.RequestFlow.Commands.CommandFlow.PipelineFlow;

public interface IStartCommandPipeline
{
    IPipelineOneCreateStep<TModel> CreateOne<TModel>(TModel model) where TModel : class;
    IPipelineOneCreateStep<TModel> CreateOne<TModel>(Func<TModel> modelFunc) where TModel : class;
    IPipelineOneCreateStep<TModel> CreateOne<TModel>(Func<Task<TModel>> modelFuncAsync) where TModel : class;
    IPipelineOneUpdateStep<TModel> UpdateOne<TModel>(Expression<Func<TModel, bool>> filter) where TModel : class;
    IPipelineOneRemoveStep<TModel> RemoveOne<TModel>(Expression<Func<TModel, bool>> filter) where TModel : class;

    // Commands with many
    IPipelineManyCreateStep<TModel> CreateMany<TModel>(IEnumerable<TModel> models) where TModel : class;
    IPipelineManyCreateStep<TModel> CreateMany<TModel>(Func<IEnumerable<TModel>> modelsFunc) where TModel : class;

    IPipelineManyCreateStep<TModel> CreateMany<TModel>(Func<Task<IEnumerable<TModel>>> modelsFuncAsync)
        where TModel : class;

    IPipelineManyUpdateStep<TModel> UpdateMany<TModel>(Expression<Func<TModel, bool>> filter) where TModel : class;
    IPipelineManyRemoveStep<TModel> RemoveMany<TModel>(Expression<Func<TModel, bool>> filter) where TModel : class;
}