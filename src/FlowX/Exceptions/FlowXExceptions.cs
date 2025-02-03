namespace FlowX.Exceptions;

public static class FlowXExceptions
{
    public class NoHandlerForRequestHasBeenRegistered(Type requestType)
        : Exception($"Not Handler for request: {requestType.Name} has been registered before!");

    public sealed class PipelineIsNotSqlPipelineBehavior(Type type) :
        Exception($"The input pipeline: {type.Name} is not matched with SqlPipeline. Please check again!");

    public sealed class NoMatchingTypeForResponse(string typeAssembly) :
        Exception($"There are no any type are matched for response: {typeAssembly}!");
    
    public sealed class CannotFindRequestType(Type requestType) :
        Exception($"Cannot find request type: {requestType.Name}!");
}