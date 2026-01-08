using System.Collections.Concurrent;
using System.Linq.Expressions;
using FlowX.Wrappers;

namespace FlowX.RabbitMq.Internal;

internal static class MessagingWrappedDynamic
{
    private static readonly ConcurrentDictionary<Type, Func<object>> MessageWrappers = new();

    internal static object NewMessagingWrapper(Type type, object message, Exception exception)
    {
        var messageWrappedFunc = MessageWrappers.GetOrAdd(type, static t =>
        {
            var ctor = typeof(MessagingWrapped<>).MakeGenericType(t).GetConstructors().First();
            var newExpression = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<object>>(newExpression);
            return lambda.Compile();
        });
        var messageWrapped = (MessagingWrapped)messageWrappedFunc.Invoke();
        messageWrapped.SetResponse(message);
        messageWrapped.SetError(exception);
        return messageWrapped;
    }
}