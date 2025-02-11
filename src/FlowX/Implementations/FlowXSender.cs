using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using FlowX.Abstractions;
using FlowX.Cached;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Implementations;

public sealed class FlowXSender(IServiceProvider serviceProvider) : IFlowXSender
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

        var pipeline = serviceProvider.GetRequiredService(serviceType);
        var method = methodInfoStorage.Value.GetOrAdd(requestType, rq => pipeline.GetType()
            .GetMethod(executeAsyncName, [typeof(IRequestXContext<>).MakeGenericType(rq)]));
        if (method is null) throw new UnreachableException();
        var headers = context?.Headers ?? [];
        var cancellationToken = context?.CancellationToken ?? CancellationToken.None;
        var requestContextType = typeof(FlowXXContext<>).MakeGenericType(requestType);
        var requestContext =
            FlowXCached.CreateInstanceWithCache(requestContextType, request, headers, cancellationToken);
        return await ((Task<TResult>)method.Invoke(pipeline, [requestContext]))!;
    }
}