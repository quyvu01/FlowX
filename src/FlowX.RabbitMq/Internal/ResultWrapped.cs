using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using FlowX.Responses;

namespace FlowX.RabbitMq.Internal;

internal static class ResultWrapped
{
    private static readonly ConcurrentDictionary<Type, ResultWrapper> Wrappers = new();

    internal static object NewResultWrapper(Type responseType, object message, Exception exception)
    {
        var wrapper = Wrappers.GetOrAdd(responseType, CreateWrapper);
        return exception == null ? wrapper.Success(message) : wrapper.Failed(exception);
    }

    private static ResultWrapper CreateWrapper(Type type)
    {
        var resultType = typeof(Result<>).MakeGenericType(type);

        // Result<T>.Success(T data)
        var successMethod = resultType
            .GetMethod(nameof(Result<object>.Success), BindingFlags.Public | BindingFlags.Static)!;

        // Result<T>.Failed(Exception ex)
        var failedMethod = resultType.GetMethod(nameof(Result<object>.Failed), [typeof(Exception), typeof(string)])!;

        // object -> object
        var dataParam = Expression.Parameter(typeof(object), "data");
        var successCall = Expression.Call(successMethod, Expression.Convert(dataParam, type));

        var successLambda = Expression
            .Lambda<Func<object, object>>(Expression.Convert(successCall, typeof(object)), dataParam)
            .Compile();

        // Exception -> object
        var exParam = Expression.Parameter(typeof(Exception), "ex");
        var failedCall = Expression.Call(failedMethod, exParam,
            Expression.Constant(null, typeof(string)));

        var failedLambda = Expression
            .Lambda<Func<Exception, object>>(
                Expression.Convert(failedCall, typeof(object)), exParam)
            .Compile();

        return new ResultWrapper(successLambda, failedLambda);
    }
}

internal sealed record ResultWrapper(
    Func<object, object> Success,
    Func<Exception, object> Failed
);