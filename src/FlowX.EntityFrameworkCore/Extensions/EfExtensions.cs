using System.Reflection;
using FlowX.Abstractions;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Registries;
using FlowX.EntityFrameworkCore.Repositories;
using FlowX.Extensions;
using FlowX.Registries;
using FlowX.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlowX.EntityFrameworkCore.Extensions;

public static class EfExtensions
{
    public static FlowXRegister AddDbContextDynamic<TDbContext>(this FlowXRegister flowXRegister,
        Action<FlowXEfCoreRegister> options) where TDbContext : DbContext
    {
        var flowXEfCoreRegister = new FlowXEfCoreRegister(flowXRegister.ServiceCollection);
        options.Invoke(flowXEfCoreRegister);
        if (flowXEfCoreRegister.IsDynamicRepositories)
            AddEfRepositoriesAsScope<TDbContext>(flowXRegister.ServiceCollection,
                FlowXStatics.ModelsFromNamespaceContaining, FlowXStatics.ModelsFilter);
        if (flowXEfCoreRegister.IsDynamicUnitOfWork)
            AddEfUnitOfWorkAsScope<TDbContext>(flowXRegister.ServiceCollection);
        // AddEfRequestHandlersAsScope(flowXRegister.ServiceCollection);
        return flowXRegister;
    }

    private static void AddEfRepositoriesAsScope<TDbContext>(IServiceCollection serviceCollection,
        Assembly modelAssembly, Func<Type, bool> modelsFilter)
        where TDbContext : DbContext
    {
        modelAssembly.ExportedTypes
            .Where(x => typeof(IEfModel).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false })
            .Where(x => modelsFilter?.Invoke(x) ?? true)
            .ForEach(modelType => serviceCollection.TryAddScoped(typeof(ISqlRepository<>).MakeGenericType(modelType),
                typeof(EfRepository<,>).MakeGenericType(typeof(TDbContext), modelType)));
    }

    private static void AddEfUnitOfWorkAsScope<TDbContext>(IServiceCollection serviceCollection)
        where TDbContext : DbContext =>
        serviceCollection.TryAddScoped(typeof(IUnitOfWork), typeof(EfUnitOfWork<TDbContext>));
}