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

    private static readonly Lazy<ConcurrentDictionary<Type, MethodInfo>> methodInfoStorage =
        new(() => []);

    public async Task<TResult> Send<TResult>(IRequest<TResult> request, Context context = null)
    {
        const string executeAsyncName = nameof(FlowPipelinesImpl<IRequest<TResult>, TResult>.ExecuteAsync);
        var requestType = request.GetType();

        var serviceType = flowPipelineStorage.Value.GetOrAdd((requestType, typeof(TResult)),
            (rq) => typeof(FlowPipelinesImpl<,>).MakeGenericType(rq.RequestType, rq.ResultType));

        var pipelineService = serviceProvider.GetRequiredService(serviceType);
        var methodInfo = methodInfoStorage.Value.GetOrAdd(requestType, rq => pipelineService.GetType()
            .GetMethod(executeAsyncName, [typeof(RequestContext<>).MakeGenericType(rq)]));
        if (methodInfo is null) throw new UnreachableException();
        var headers = context?.Headers ?? [];
        var cancellationToken = context?.CancellationToken ?? CancellationToken.None;
        var requestContextType = typeof(FlowContext<>).MakeGenericType(requestType);
        var requestContext =
            FlowXCached.CreateInstanceWithCache(requestContextType, request, headers, cancellationToken);
        return await ((Task<TResult>)methodInfo.Invoke(pipelineService, [requestContext]))!;
    }
}