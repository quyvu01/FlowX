namespace FlowX.Exceptions;

public static class FlowXExceptions
{
    public sealed class PipelineIsNotPipelineBehavior(Type type) :
        Exception($"The input pipeline: {type.Name} is not matched with Pipeline. Please check again!");

    public sealed class RequestIsNotRequestBase(Type requestType) :
        Exception($"Request is not IRequestBase: {requestType.Name}!");
}