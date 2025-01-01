using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Registries;

public class FlowXRegister(IServiceCollection serviceCollection)
{
    public Assembly HandlersRegister { get; private set; }
    public Assembly ModelsAssemblyRegister { get; private set; }
    public IServiceCollection ServiceCollection { get; } = serviceCollection;

    public void AddModelsFromNamespaceContaining<TAssemblyMarker>() =>
        ModelsAssemblyRegister = typeof(TAssemblyMarker).Assembly;

    public void AddHandlersContainNamespaces(Assembly handlersAssembly) =>
        HandlersRegister = handlersAssembly;
}