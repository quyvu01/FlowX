using FlowX.Statics;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Registries;

public class FlowXRegister(IServiceCollection serviceCollection)
{
    public IServiceCollection ServiceCollection { get; } = serviceCollection;

    public void AddHandlersFromNamespaceContaining<TAssemblyMarker>() =>
        FlowXStatics.HandlersFromNamespaceContaining = typeof(TAssemblyMarker).Assembly;
}