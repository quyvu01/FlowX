namespace FlowX.Exceptions;

public static class FlowXExceptions
{
    public class NoHandlerForRequestHasBeenRegistered(Type requestType)
        : Exception($"Not Handler for request: {requestType.Name} has been registered before!");

    public sealed class PipelineIsNotSqlPipelineBehavior(Type type) :
        Exception($"The input pipeline: {type.Name} is not matched with SqlPipeline. Please check again!");
}