using FlowX.ApplicationModels;
using FlowX.Statics;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Registries;

public class FlowXRegister(IServiceCollection serviceCollection)
{
    public IServiceCollection ServiceCollection { get; } = serviceCollection;

    public void AddModelsFromNamespaceContaining<TAssemblyMarker>(Func<Type, bool> modelsFilter = null) =>
        (FlowXStatics.ModelsFromNamespaceContaining, FlowXStatics.ModelsFilter) =
        (typeof(TAssemblyMarker).Assembly, modelsFilter);

    public void AddHandlersFromNamespaceContaining<TAssemblyMarker>() =>
        FlowXStatics.HandlersFromNamespaceContaining = typeof(TAssemblyMarker).Assembly;

    public void SetRequestTimeOut(TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeout, TimeSpan.Zero);
        FlowXStatics.DefaultRequestTimeout = timeout;
    }

    public void SetRetryPolicy(int retryCount = 3, Func<int, TimeSpan> sleepDurationProvider = null,
        Func<Exception, TimeSpan, IServiceProvider, Task> onRetryAsync = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(retryCount, 0);
        FlowXStatics.RetryPolicy = new RetryPolicy(retryCount, sleepDurationProvider, onRetryAsync);
    }
}