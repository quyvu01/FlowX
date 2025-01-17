using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Registries;

public class FlowXRegister(IServiceCollection serviceCollection)
{
    public Func<Type, bool> ModelsFilter { get; private set; }
    public Assembly HandlersFromNamespaceContaining { get; private set; }
    public Assembly ModelsFromNamespaceContaining { get; private set; }
    public IServiceCollection ServiceCollection { get; } = serviceCollection;

    public void AddModelsFromNamespaceContaining<TAssemblyMarker>(Func<Type, bool> modelsFilter = null) =>
        (ModelsFromNamespaceContaining, ModelsFilter) = (typeof(TAssemblyMarker).Assembly, modelsFilter);

    public void AddHandlersFromNamespaceContaining<TAssemblyMarker>() =>
        HandlersFromNamespaceContaining = typeof(TAssemblyMarker).Assembly;
}