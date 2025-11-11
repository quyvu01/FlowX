using System.Collections.Concurrent;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Delegates;
using FlowX.EntityFrameworkCore.Exceptions;
using FlowX.EntityFrameworkCore.InternalPipelines;
using FlowX.EntityFrameworkCore.Registries;
using FlowX.EntityFrameworkCore.Repositories;
using FlowX.Extensions;
using FlowX.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlowX.EntityFrameworkCore.Extensions;

public static class EfExtensions
{
    public static FlowXRegisterWrapped AddEfCore(this FlowXRegisterWrapped flowXRegisterWrapped,
        Action<FlowXEfCoreRegister> options)
    {
        var serviceCollection = flowXRegisterWrapped.FlowXRegister.ServiceCollection;
        var flowXEfCoreRegister = new FlowXEfCoreRegister(serviceCollection);
        options.Invoke(flowXEfCoreRegister);

        serviceCollection.AddScoped<GetDbContexts>(sp =>
            () => [..sp.GetServices<IDbContext>().Select(a => a.DbContext)]);

        var modelCacheLookup = new ConcurrentDictionary<Type, int>();

        serviceCollection.AddScoped<GetDbContext>(sp => modelType =>
        {
            var modelCached = modelCacheLookup.GetOrAdd(modelType, mt =>
            {
                var ofXDbContexts = sp.GetServices<IDbContext>().ToList();
                var internalDbContext = ofXDbContexts.FirstOrDefault(x => x.HasCollection(mt));
                return internalDbContext is null
                    ? throw new FlowXEntityFrameworkException.ModelDoesNotBelongToDbContext(modelType)
                    : ofXDbContexts.IndexOf(internalDbContext);
            });
            var iDbContext = sp.GetServices<IDbContext>().Skip(modelCached).First();
            return iDbContext.DbContext;
        });

        serviceCollection.AddScoped<IUnitOfWork, EfUnitOfWork>();
        serviceCollection.AddScoped(typeof(EfRepository<>));
        flowXRegisterWrapped.FlowXRegister
            .AddPipelines(c => c.OfType(typeof(UnitOfWorkStatePipeline<,>)));
        return flowXRegisterWrapped;
    }

    private static void AddEfUnitOfWorkAsScope(IServiceCollection serviceCollection) =>
        serviceCollection.TryAddScoped<IUnitOfWork, EfUnitOfWork>();
}