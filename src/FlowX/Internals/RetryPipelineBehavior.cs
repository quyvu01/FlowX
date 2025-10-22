using FlowX.Abstractions;
using FlowX.Statics;

namespace FlowX.Internals;

internal sealed class RetryPipelineBehavior<TRequest, TResult>(IServiceProvider serviceProvider)
    : IFlowPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public async Task<TResult> HandleAsync(IRequestContext<TRequest> requestContext, Func<Task<TResult>> next)
    {
        var retryPolicy = FlowXStatics.RetryPolicy;
        if (retryPolicy is null) return await next.Invoke();
        try
        {
            return await next.Invoke();
        }
        catch (Exception)
        {
            foreach (var retryCount in Enumerable.Range(1, retryPolicy.RetryCount))
            {
                try
                {
                    return await next.Invoke();
                }
                catch (Exception ex)
                {
                    var retryAfter = TimeSpan.Zero;
                    if (retryPolicy.SleepDurationProvider is { } sleepDurationProvider)
                    {
                        retryAfter = sleepDurationProvider.Invoke(retryCount);
                        await Task.Delay(sleepDurationProvider.Invoke(retryCount));
                    }

                    if (retryPolicy.OnRetryAsync is { } onRetryAsync)
                        await onRetryAsync.Invoke(ex, retryAfter, serviceProvider);
                }
            }

            throw;
        }
    }
}