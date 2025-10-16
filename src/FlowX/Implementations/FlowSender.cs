using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using FlowX.Abstractions;
using FlowX.Cached;
using FlowX.Exceptions;
using FlowX.Externals;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Implementations;

internal sealed class FlowSender(IServiceProvider serviceProvider) : IFlow
{
    private static readonly Lazy<ConcurrentDictionary<(Type RequestType, Type ResultType), Type>> FlowPipelineLookup =
        new(() => []);

    private static readonly Lazy<ConcurrentDictionary<Type, Type>> RequestMapResponseType =
        new(() => []);

    private static readonly Lazy<ConcurrentDictionary<Type, MethodInfo>> MethodInfoLookup =
        new(() => []);

    public async Task<TResult> Send<TResult>(IRequest<TResult> request,
        CancellationToken cancellationToken = default)
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
        const string executeAsyncName = nameof(FlowPipelinesImpl<IRequest<TResult>, TResult>.ExecuteAsync);
        var requestType = request.GetType();
        var serviceType = FlowPipelineLookup.Value.GetOrAdd((requestType, typeof(TResult)),
            rq => typeof(FlowPipelinesImpl<,>).MakeGenericType(rq.RequestType, rq.ResultType));

        var pipelineService = serviceProvider.GetRequiredService(serviceType);
        var methodInfo = MethodInfoLookup.Value.GetOrAdd(requestType, rq => pipelineService.GetType()
            .GetMethod(executeAsyncName, [typeof(IRequestContext<>).MakeGenericType(rq)]));
        if (methodInfo is null) throw new UnreachableException();
        var headers = context.Headers;
        var cancellationToken = context.CancellationToken;
        var requestContextType = typeof(FlowContext<>).MakeGenericType(requestType);
        var requestContext = FlowXCached
            .CreateInstanceWithCache(requestContextType, request, headers, cancellationToken);
        return await ((Task<TResult>)methodInfo.Invoke(pipelineService, [requestContext]))!;
    }

    private async Task<object> Send(object request, FlowXContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestType = request.GetType();
        if (request is not IRequestBase) throw new FlowXExceptions.RequestIsNotRequestBase(requestType);
        var resultType = RequestMapResponseType.Value.GetOrAdd(requestType, rq =>
        {
            var interfaceType = rq
                .GetInterfaces()
                .FirstOrDefault(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(IRequest<>));
            return interfaceType is null
                ? throw new FlowXExceptions.RequestIsNotRequestBase(requestType)
                : interfaceType.GetGenericArguments()[0];
        });

        var serviceType = FlowPipelineLookup.Value.GetOrAdd((requestType, resultType),
            rq => typeof(FlowPipelinesImpl<,>).MakeGenericType(rq.RequestType, rq.ResultType));

        var pipelineService = serviceProvider.GetRequiredService(serviceType);
        var methodInfo = MethodInfoLookup.Value.GetOrAdd(requestType, rq => pipelineService.GetType()
            .GetMethod("ExecuteAsync", [typeof(IRequestContext<>).MakeGenericType(rq)]));
        if (methodInfo is null) throw new UnreachableException();
        var headers = context.Headers;
        var cancellationToken = context.CancellationToken;
        var requestContextType = typeof(FlowContext<>).MakeGenericType(requestType);
        var requestContext = FlowXCached
            .CreateInstanceWithCache(requestContextType, request, headers, cancellationToken);

        var task = (Task)methodInfo.Invoke(pipelineService, [requestContext]);
        if (task is null) throw new UnreachableException();
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        return resultProperty is null
            ? throw new InvalidOperationException("Method did not return a Task with a result.")
            : resultProperty.GetValue(task);
    }
}