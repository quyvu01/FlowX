using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using FlowX.Abstractions;
using FlowX.Cached;
using FlowX.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Implementations;

internal sealed class FlowSender(IServiceProvider serviceProvider) : IFlow
{
    private static readonly Lazy<ConcurrentDictionary<(Type RequestType, Type ResultType), Type>> flowPipelineStorage =
        new(() => []);

    private static readonly Lazy<ConcurrentDictionary<Type, Type>> requestMapResponseType =
        new(() => []);

    private static readonly Lazy<ConcurrentDictionary<Type, MethodInfo>> methodInfoStorage =
        new(() => []);


    public async Task<TResult> Send<TResult>(IRequest<TResult> request, Context context = null)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        const string executeAsyncName = nameof(FlowPipelinesImpl<IRequest<TResult>, TResult>.ExecuteAsync);
        var requestType = request.GetType();

        var serviceType = flowPipelineStorage.Value.GetOrAdd((requestType, typeof(TResult)),
            rq => typeof(FlowPipelinesImpl<,>).MakeGenericType(rq.RequestType, rq.ResultType));

        var pipelineService = serviceProvider.GetRequiredService(serviceType);
        var methodInfo = methodInfoStorage.Value.GetOrAdd(requestType, rq => pipelineService.GetType()
            .GetMethod(executeAsyncName, [typeof(RequestContext<>).MakeGenericType(rq)]));
        if (methodInfo is null) throw new UnreachableException();
        var headers = context?.Headers ?? [];
        var cancellationToken = context?.CancellationToken ?? CancellationToken.None;
        var requestContextType = typeof(FlowContext<>).MakeGenericType(requestType);
        var requestContext = FlowXCached
            .CreateInstanceWithCache(requestContextType, request, headers, cancellationToken);
        return await ((Task<TResult>)methodInfo.Invoke(pipelineService, [requestContext]))!;
    }

    // Todo: Temp add Send method with object to test. Update later!
    public async Task<object> Send(object request, Context context = null)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var requestType = request.GetType();
        if (request is not IRequestBase) throw new FlowXExceptions.RequestIsNotRequestBase(requestType);
        var resultType = requestMapResponseType.Value.GetOrAdd(requestType, rq =>
        {
            var interfaceType = rq
                .GetInterfaces()
                .FirstOrDefault(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(IRequest<>));
            if (interfaceType is null) throw new FlowXExceptions.RequestIsNotRequestBase(requestType);
            return interfaceType.GetGenericArguments()[0];
        });

        var serviceType = flowPipelineStorage.Value.GetOrAdd((requestType, resultType),
            rq => typeof(FlowPipelinesImpl<,>).MakeGenericType(rq.RequestType, rq.ResultType));

        var pipelineService = serviceProvider.GetRequiredService(serviceType);
        var methodInfo = methodInfoStorage.Value.GetOrAdd(requestType, rq => pipelineService.GetType()
            .GetMethod("ExecuteAsync", [typeof(RequestContext<>).MakeGenericType(rq)]));
        if (methodInfo is null) throw new UnreachableException();
        var headers = context?.Headers ?? [];
        var cancellationToken = context?.CancellationToken ?? CancellationToken.None;
        var requestContextType = typeof(FlowContext<>).MakeGenericType(requestType);
        var requestContext = FlowXCached
            .CreateInstanceWithCache(requestContextType, request, headers, cancellationToken);

        var task = (Task)methodInfo.Invoke(pipelineService, [requestContext]);
        if (task is null) throw new UnreachableException();
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        if (resultProperty is null) throw new InvalidOperationException("Method did not return a Task with a result.");
        return resultProperty.GetValue(task);
    }
}