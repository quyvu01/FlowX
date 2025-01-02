using System.Reflection;
using System.Reflection.Emit;
using FlowX.Abstractions;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Registries;
using FlowX.EntityFrameworkCore.Repositories;
using FlowX.Extensions;
using FlowX.Registries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

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
                flowXRegister.ModelsFromNamespaceContaining, flowXRegister.ModelsFilter);
        if (flowXEfCoreRegister.IsDynamicUnitOfWork)
            AddEfUnitOfWorkAsScope<TDbContext>(flowXRegister.ServiceCollection);
        return flowXRegister;
    }

    private static void AddEfRepositoriesAsScope<TDbContext>(IServiceCollection services,
        Assembly modelAssembly, Func<Type, bool> modelsFilter)
        where TDbContext : DbContext => modelAssembly.ExportedTypes
        .Where(x => typeof(IEfModel).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false })
        .Where(x => modelsFilter?.Invoke(x) ?? true)
        .ForEach(modelType =>
        {
            var assemblyName = new AssemblyName
                { Name = $"{typeof(TDbContext).Assembly.GetName().Name}.DynamicEfRepositoryType" };
            var newAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var newModule = newAssembly.DefineDynamicModule("DynamicEfRepositoryModule");
            var efRepositoryType = typeof(EfRepository<>).MakeGenericType(modelType);
            var typeBuilder = newModule
                .DefineType($"{modelType.Name}Repository", TypeAttributes.Public, efRepositoryType);
            var ctorTypes = efRepositoryType.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance, [typeof(TDbContext)])!;
            // Define the constructor for the dynamic class
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard, [typeof(TDbContext)]);
            // Generate the constructor IL code
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0); // Load "this" onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_1); // Load the TDbContext argument onto the stack
            // Call the base constructor with the TDbContext and ILogger arguments
            ilGenerator.Emit(OpCodes.Call, ctorTypes);
            ilGenerator.Emit(OpCodes.Ret); // Return from the constructor
            var repositoryType = typeBuilder.CreateType();
            var implementationType = typeof(ISqlRepository<>).MakeGenericType(modelType);
            services.TryAddScoped(implementationType, repositoryType);
        });

    private static void AddEfUnitOfWorkAsScope<TDbContext>(IServiceCollection services) where TDbContext : DbContext
    {
        var assemblyName = new AssemblyName
            { Name = $"{typeof(TDbContext).Assembly.GetName().Name}.DynamicEfUnitOfWorkType" };
        var newAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var newModule = newAssembly.DefineDynamicModule("DynamicEfUnitOfWorkModule");
        var efUnitOfWorkType = typeof(EfUnitOfWork);
        var typeBuilder = newModule.DefineType("UnitOfWork", TypeAttributes.Public, efUnitOfWorkType);
        var ctorTypes = efUnitOfWorkType
            .GetConstructor(BindingFlags.Public | BindingFlags.Instance, [typeof(TDbContext)])!;
        // Define the constructor for the dynamic class
        var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
            CallingConventions.Standard, [typeof(TDbContext)]);
        // Generate the constructor IL code
        var ilGenerator = constructorBuilder.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0); // Load "this" onto the stack
        ilGenerator.Emit(OpCodes.Ldarg_1); // Load the TDbContext argument onto the stack
        // Call the base constructor with the TDbContext and ILogger arguments
        ilGenerator.Emit(OpCodes.Call, ctorTypes);
        ilGenerator.Emit(OpCodes.Ret); // Return from the constructor
        var unitOfWorkType = typeBuilder.CreateType();
        services.TryAddScoped(typeof(IUnitOfWork), unitOfWorkType);
    }
}