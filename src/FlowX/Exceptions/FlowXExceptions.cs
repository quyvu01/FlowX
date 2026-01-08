namespace FlowX.Exceptions;

public static class FlowXExceptions
{
    public sealed class PipelineIsNotPipelineBehavior(Type type)
        : Exception(
            $"Invalid pipeline type detected: {type.FullName}. The specified type must implement IPipelineBehavior<> to be used as part of the processing pipeline.");

    public sealed class RequestIsNotRequestBase(Type requestType)
        : Exception(
            $"Invalid request type: {requestType.FullName}. All requests must implement IRequestBase to be recognized by the request processing system.");


    public sealed class AmbiguousRequestType(Type requestType)
        : Exception(
            $"Ambiguous request type detected: {requestType.FullName}. A single class cannot implement multiple IRequest<> interfaces.");


    public sealed class RequestDoesNotMatchWithResponse(Type requestType)
        : Exception($"Cannot find response type for Request: {requestType.FullName}.");
}