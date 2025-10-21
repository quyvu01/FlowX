using System.Collections.Concurrent;
using System.Linq.Expressions;
using FlowX.Abstractions;
using FlowX.Exceptions;
using FlowX.Externals;
using FlowX.Wrappers;

namespace FlowX.Implementations;

internal sealed class FlowSender(IServiceProvider serviceProvider) : IFlow
{
    private static readonly ConcurrentDictionary<Type, Func<RequestHandlerWrapperBase>> _requestHandlers =
        new();

    public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        => await Send(request, new FlowXContext([], cancellationToken)).ConfigureAwait(false);

    public async Task<object> Send(object request, CancellationToken cancellationToken = default)
        => await Send(request, new FlowXContext([], cancellationToken)).ConfigureAwait(false);

    private async Task<TResult> Send<TResult>(IRequest<TResult> request, FlowXContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        var func = _requestHandlers.GetOrAdd(request.GetType(), static requestType =>
        {
            var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResult));
            var ctor = wrapperType.GetConstructor(Type.EmptyTypes);
            ArgumentNullException.ThrowIfNull(ctor);
            var exp = Expression
                .Lambda<Func<RequestHandlerWrapperBase>>(Expression.New(ctor)).Compile();
            return exp;
        });

        var requestHandlerWrapper = (RequestHandlerWrapper<TResult>)func.Invoke();

        return await requestHandlerWrapper.HandleAsync(request, serviceProvider,
            context?.CancellationToken ?? CancellationToken.None);
    }

    private async Task<object> Send(object request, FlowXContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request is not IRequestBase) throw new FlowXExceptions.RequestIsNotRequestBase(request.GetType());
        var handlerWrapper = _requestHandlers.GetOrAdd(request.GetType(), static rq =>
        {
            var interfaceType = rq
                .GetInterfaces()
                .FirstOrDefault(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(IRequest<>));
            if (interfaceType is null) throw new FlowXExceptions.RequestIsNotRequestBase(rq);
            var responseType = interfaceType.GetGenericArguments()[0];
            var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(rq, responseType);
            var ctor = wrapperType.GetConstructor(Type.EmptyTypes);
            ArgumentNullException.ThrowIfNull(ctor);
            var exp = Expression
                .Lambda<Func<RequestHandlerWrapperBase>>(Expression.New(ctor)).Compile();
            return exp;
        });

        var handler = handlerWrapper.Invoke();
        return await handler.HandleAsync(request, serviceProvider,
            context?.CancellationToken ?? CancellationToken.None);
    }
}