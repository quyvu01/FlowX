using System.Diagnostics;
using FlowX.Abstractions;
using FlowX.Cached;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Implementations;

public sealed class FlowXSender(IServiceProvider serviceProvider) : IFlowXSender
{
    public async Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, FlowContext context = null)
    {
        const string executeAsyncName = nameof(FlowPipelinesImpl<IRequest<TResult>, TResult>.ExecuteAsync);
        var requestType = request.GetType();

        var pipeline = serviceProvider
            .GetRequiredService(typeof(FlowPipelinesImpl<,>).MakeGenericType(requestType, typeof(TResult)));
        var method = pipeline.GetType()
            .GetMethod(executeAsyncName, [typeof(RequestContext<>).MakeGenericType(requestType)]);
        if (method is null) throw new UnreachableException();
        var headers = context?.Headers ?? [];
        var cancellationToken = context?.CancellationToken ?? CancellationToken.None;
        var requestContextType = typeof(FlowXContext<>).MakeGenericType(requestType);
        var requestContext =
            FlowXCached.CreateInstanceWithCache(requestContextType, request, headers, cancellationToken);
        return await ((Task<TResult>)method.Invoke(pipeline, [requestContext]))!;
    }
}