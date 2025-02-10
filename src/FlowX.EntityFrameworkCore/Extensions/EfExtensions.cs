using System.Reflection;
using System.Reflection.Emit;
using FlowX.Abstractions;
using FlowX.Cached;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.Exceptions;
using FlowX.EntityFrameworkCore.Registries;
using FlowX.EntityFrameworkCore.Repositories;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandMany;
using FlowX.EntityFrameworkCore.RequestHandlers.Commands.CommandOne;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryCounting;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;
using FlowX.Errors;
using FlowX.Extensions;
using FlowX.Registries;
using FlowX.Responses;
using FlowX.Structs;
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
                flowXRegister.ModelsFromNamespaceContaining, flowXRegister.ModelsFilter);
        if (flowXEfCoreRegister.IsDynamicUnitOfWork)
            AddEfUnitOfWorkAsScope<TDbContext>(flowXRegister.ServiceCollection);
        AddEfRequestHandlersAsScope(flowXRegister.ServiceCollection, flowXRegister.HandlersFromNamespaceContaining);
        return flowXRegister;
    }

    private static void AddEfRepositoriesAsScope<TDbContext>(IServiceCollection serviceCollection,
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
            serviceCollection.TryAddScoped(implementationType, repositoryType);
        });

    private static void AddEfUnitOfWorkAsScope<TDbContext>(IServiceCollection serviceCollection)
        where TDbContext : DbContext
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
        serviceCollection.TryAddScoped(typeof(IUnitOfWork), unitOfWorkType);
    }

    private static void AddEfRequestHandlersAsScope(IServiceCollection serviceCollection,
        Assembly handlersAssembly)
    {
        handlersAssembly.ExportedTypes
            .Where(x => typeof(IRequestHandlerBase).IsAssignableFrom(x) &&
                        x is { IsInterface: false, IsAbstract: false })
            .ForEach(handlerType =>
            {
                var basedType = handlerType.BaseType;
                if (basedType is null) return;
                if (!basedType.IsGenericType) return;
                var genericTypeDefinition = basedType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(EfQueryOneHandler<,,>))
                {
                    var arguments = basedType.GetGenericArguments();
                    var requestType = arguments[1];
                    var resultType = typeof(OneOf<,>).MakeGenericType(arguments[2], typeof(Error));
                    var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                    if (serviceCollection.Any(a => a.ServiceType == serviceType))
                        throw new FlowXEntityFrameworkException.RequestHasBeenRegistered(arguments[1]);
                    FlowXCached.RequestMapResponse.Value.TryAdd(requestType, resultType);
                    serviceCollection.TryAddScoped(serviceType, handlerType);
                    return;
                }

                if (genericTypeDefinition == typeof(EfQueryCollectionHandler<,,>))
                {
                    var arguments = basedType.GetGenericArguments();

                    var requestType = arguments[1];
                    var resultType = typeof(CollectionResponse<>).MakeGenericType(arguments[2]);
                    var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                    if (serviceCollection.Any(a => a.ServiceType == serviceType))
                        throw new FlowXEntityFrameworkException.RequestHasBeenRegistered(arguments[1]);
                    FlowXCached.RequestMapResponse.Value.TryAdd(requestType, resultType);
                    serviceCollection.TryAddScoped(serviceType, handlerType);
                    return;
                }

                if (genericTypeDefinition == typeof(EfQueryPaginationHandler<,,>))
                {
                    var arguments = basedType.GetGenericArguments();
                    var requestType = arguments[1];
                    var resultType = typeof(PaginationResponse<>).MakeGenericType(arguments[2]);
                    var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                    if (serviceCollection.Any(a => a.ServiceType == serviceType))
                        throw new FlowXEntityFrameworkException.RequestHasBeenRegistered(arguments[1]);
                    FlowXCached.RequestMapResponse.Value.TryAdd(requestType, resultType);
                    serviceCollection.TryAddScoped(serviceType, handlerType);
                    return;
                }

                if (genericTypeDefinition == typeof(EfQueryCountingHandler<,>))
                {
                    var arguments = basedType.GetGenericArguments();

                    var requestType = arguments[1];
                    var resultType = typeof(CountingResponse);
                    var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                    if (serviceCollection.Any(a => a.ServiceType == serviceType))
                        throw new FlowXEntityFrameworkException.RequestHasBeenRegistered(arguments[1]);
                    FlowXCached.RequestMapResponse.Value.TryAdd(requestType, resultType);
                    serviceCollection.TryAddScoped(serviceType, handlerType);
                    return;
                }

                if (genericTypeDefinition == typeof(EfCommandOneVoidHandler<,>) ||
                    genericTypeDefinition == typeof(EfCommandManyVoidHandler<,>))
                {
                    var arguments = basedType.GetGenericArguments();
                    var requestType = arguments[1];
                    var resultType = typeof(OneOf<,>).MakeGenericType(typeof(None), typeof(Error));
                    var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                    if (serviceCollection.Any(a => a.ServiceType == serviceType))
                        throw new FlowXEntityFrameworkException.RequestHasBeenRegistered(arguments[1]);
                    FlowXCached.RequestMapResponse.Value.TryAdd(requestType, resultType);
                    serviceCollection.TryAddScoped(serviceType, handlerType);
                    return;
                }

                if (genericTypeDefinition == typeof(EfCommandOneResultHandler<,,>) ||
                    genericTypeDefinition == typeof(EfCommandManyResultHandler<,,>))
                {
                    var arguments = basedType.GetGenericArguments();

                    var requestType = arguments[1];
                    var resultType = typeof(OneOf<,>).MakeGenericType(arguments[2], typeof(Error));
                    var serviceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                    if (serviceCollection.Any(a => a.ServiceType == serviceType))
                        throw new FlowXEntityFrameworkException.RequestHasBeenRegistered(arguments[1]);
                    FlowXCached.RequestMapResponse.Value.TryAdd(requestType, resultType);
                    serviceCollection.TryAddScoped(serviceType, handlerType);
                }
            });
    }
}