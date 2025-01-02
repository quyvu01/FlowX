using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Tests;

public abstract class ServicesBuilding
{
    protected IServiceProvider ServiceProvider { get; private set; }
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceInstallers = [];

    private readonly IConfiguration _configuration = new ConfigurationBuilder().Build();

    private void BuildServiceProvider() => ServiceProvider = _serviceCollection.BuildServiceProvider();

    public ServicesBuilding InstallService(Action<IServiceCollection, IConfiguration> installingAction)
    {
        _serviceInstallers.Add(installingAction);
        return this;
    }


    public void InstallAllServices()
    {
        _serviceInstallers.ForEach(s => s.Invoke(_serviceCollection, _configuration));
        BuildServiceProvider();
    }
}