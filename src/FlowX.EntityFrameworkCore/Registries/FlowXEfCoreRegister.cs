using Microsoft.Extensions.DependencyInjection;

namespace FlowX.EntityFrameworkCore.Registries;

public class FlowXEfCoreRegister(IServiceCollection serviceCollection)
{
    public bool IsDynamicRepositories { get; private set; }
    public bool IsDynamicUnitOfWork { get; private set; }
    public IServiceCollection ServiceCollection { get; } = serviceCollection;
    public void AddDynamicRepositories() => IsDynamicRepositories = true;
    public void AddDynamicUnitOfWork() => IsDynamicUnitOfWork = true;
}