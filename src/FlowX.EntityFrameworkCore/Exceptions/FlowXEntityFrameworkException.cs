namespace FlowX.EntityFrameworkCore.Exceptions;

public static class FlowXEntityFrameworkException
{
    public class RequestHasBeenRegistered(Type requestType)
        : Exception($"Request: {requestType.Name} has been registered before!");
}