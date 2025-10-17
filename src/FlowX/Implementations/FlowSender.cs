using System.Collections.Concurrent;
using FlowX.Abstractions;
using FlowX.Exceptions;
using FlowX.Externals;
using FlowX.Wrappers;

namespace FlowX.Implementations;

internal sealed class FlowSender(IServiceProvider serviceProvider) : IFlow
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapperBase> _RequestHandlerWrapperBases = new();

    public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await Send(request, new FlowXContext([], cancellationToken)).ConfigureAwait(false);
        return result;
    }

    public async Task<object> Send(object request, CancellationToken cancellationToken = default)
    {
        var result = await Send(request, new FlowXContext([], cancellationToken)).ConfigureAwait(false);
        return result;
    }

    private async Task<TResult> Send<TResult>(IRequest<TResult> request, FlowXContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        var handler = (RequestHandlerWrapper<TResult>)_RequestHandlerWrapperBases.GetOrAdd(request.GetType(),
            static requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResult));
                var wrapper = Activator.CreateInstance(wrapperType) ??
                              throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                return (RequestHandlerWrapperBase)wrapper;
            });

        return await handler.HandleAsync(request, serviceProvider,
            context?.CancellationToken ?? CancellationToken.None);
    }

    private async Task<object> Send(object request, FlowXContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestType = request.GetType();
        if (request is not IRequestBase) throw new FlowXExceptions.RequestIsNotRequestBase(requestType);
        var handlerWrapper = _RequestHandlerWrapperBases.GetOrAdd(requestType, rq =>
        {
            var interfaceType = rq
                .GetInterfaces()
                .FirstOrDefault(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(IRequest<>));
            if (interfaceType is null) throw new FlowXExceptions.RequestIsNotRequestBase(requestType);
            var responseType = interfaceType.GetGenericArguments()[0];
            var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
            var wrapper = Activator.CreateInstance(wrapperType) ??
                          throw new InvalidOperationException($"Could not create wrapper for type {requestType}");
            return (RequestHandlerWrapperBase)wrapper;
        });

        return await handlerWrapper.HandleAsync(request, serviceProvider,
            context?.CancellationToken ?? CancellationToken.None);
    }
}