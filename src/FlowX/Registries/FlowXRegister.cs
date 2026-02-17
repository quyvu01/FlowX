using System.Reflection;
using FlowX.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Registries;

public class FlowXRegister(IServiceCollection serviceCollection)
{
    public IServiceCollection ServiceCollection { get; } = serviceCollection;
    internal HashSet<Assembly> HandlersFromNamespacesContaining { get; } = [];

    public void AddHandlersFromNamespaceContaining<TAssemblyMarker>() =>
        HandlersFromNamespacesContaining.Add(typeof(TAssemblyMarker).Assembly);

    public void AddHandlersFromNamespaceContaining(params Assembly[] assemblies) =>
        assemblies.ForEach(assembly => HandlersFromNamespacesContaining.Add(assembly));
}