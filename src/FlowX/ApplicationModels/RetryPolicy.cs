namespace FlowX.ApplicationModels;

internal record RetryPolicy(
    int RetryCount,
    Func<int, TimeSpan> SleepDurationProvider,
    Func<Exception, TimeSpan, IServiceProvider, Task> OnRetryAsync);