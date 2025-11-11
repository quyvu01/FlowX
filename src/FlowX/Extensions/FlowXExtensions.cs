using System.Reflection;
using FlowX.Abstractions;
using FlowX.Cached;
using FlowX.Implementations;
using FlowX.Internals;
using FlowX.Registries;
using FlowX.Statics;
using FlowX.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlowX.Extensions;

public static class FlowXExtensions
{
    private const int MaxGenericTypeParameters = 10;
    private const int MaxTypesClosing = 100;
    private const int MaxGenericTypeRegistrations = 125000;

    public static FlowXRegisterWrapped AddFlowX(this IServiceCollection serviceCollection,
        Action<FlowXRegister> options)
    {
        var newFlowXRegister = new FlowXRegister(serviceCollection);
        options.Invoke(newFlowXRegister);
        serviceCollection.AddTransient(typeof(FlowPipelinesImpl<,>));
        serviceCollection.AddTransient<IMediator, MediatorSender>();
        ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>), serviceCollection,
            [FlowXStatics.HandlersFromNamespaceContaining], false, CancellationToken.None);
        serviceCollection
            .ForEach(c =>
            {
                if (!c.ServiceType.IsGenericType) return;
                var genericTypeDefinition = c.ServiceType.GetGenericTypeDefinition();
                if (genericTypeDefinition != typeof(IRequestHandler<,>)) return;
                var args = c.ServiceType.GetGenericArguments();
                FlowXCached.InternalRequestMapResponse.Value.TryAdd(args.First(), args.Last());
            });
        serviceCollection.AddTransient(typeof(IRequestHandler<,>), typeof(DefaultRequestHandler<,>));
        newFlowXRegister.AddPipelines(c => c
            .OfType(typeof(TransportPipeline<,>))
            .OfType(typeof(PagedPipeline<,>))
        );
        return new FlowXRegisterWrapped(newFlowXRegister);
    }

    private static void ConnectImplementationsToTypesClosing(Type openRequestInterface,
        IServiceCollection services,
        ICollection<Assembly> assembliesToScan,
        bool addIfAlreadyExists,
        CancellationToken cancellationToken = default)
    {
        var concretions = new List<Type>();
        var interfaces = new List<Type>();
        var genericConcretions = new List<Type>();
        var genericInterfaces = new List<Type>();

        var types = assembliesToScan
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.ContainsGenericParameters)
            .Where(t => t.IsConcrete() && t.FindInterfacesThatClose(openRequestInterface).Any())
            .ToList();

        foreach (var type in types)
        {
            var interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();

            if (!type.IsOpenGeneric())
            {
                concretions.Add(type);

                foreach (var interfaceType in interfaceTypes) interfaces.Fill(interfaceType);
                continue;
            }

            genericConcretions.Add(type);
            foreach (var interfaceType in interfaceTypes) genericInterfaces.Fill(interfaceType);
        }

        foreach (var @interface in interfaces)
        {
            var exactMatches = concretions.Where(x => x.CanBeCastTo(@interface)).ToList();
            if (addIfAlreadyExists)
            {
                foreach (var type in exactMatches) services.AddTransient(@interface, type);
                continue;
            }

            if (exactMatches.Count > 1) exactMatches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));

            foreach (var type in exactMatches) services.TryAddTransient(@interface, type);

            if (!@interface.IsOpenGeneric()) AddConcretionsThatCouldBeClosed(@interface, concretions, services);
        }

        foreach (var @interface in genericInterfaces)
        {
            var exactMatches = genericConcretions.Where(x => x.CanBeCastTo(@interface)).ToList();
            AddAllConcretionsThatClose(@interface, exactMatches, services, assembliesToScan, cancellationToken);
        }
    }

    private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions,
        IServiceCollection services)
    {
        foreach (var type in concretions.Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
        {
            try
            {
                services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private static void AddAllConcretionsThatClose(Type openRequestInterface, List<Type> concretions,
        IServiceCollection services, ICollection<Assembly> assembliesToScan, CancellationToken cancellationToken)
    {
        foreach (var concretion in concretions)
        {
            var concreteRequests =
                GetConcreteRequestTypes(openRequestInterface, concretion, assembliesToScan, cancellationToken);

            if (concreteRequests is null) continue;

            var registrationTypes = concreteRequests
                .Select(concreteRequest =>
                    GetConcreteRegistrationTypes(openRequestInterface, concreteRequest, concretion));

            foreach (var (service, implementation) in registrationTypes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                services.AddTransient(service, implementation);
            }
        }
    }

    private static (Type Service, Type Implementation) GetConcreteRegistrationTypes(Type openRequestHandlerInterface,
        Type concreteGenericTRequest, Type openRequestHandlerImplementation)
    {
        var closingTypes = concreteGenericTRequest.GetGenericArguments();

        var concreteTResponse = concreteGenericTRequest.GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequest<>))
            ?.GetGenericArguments()
            .FirstOrDefault();

        var typeDefinition = openRequestHandlerInterface.GetGenericTypeDefinition();

        var serviceType = concreteTResponse != null
            ? typeDefinition.MakeGenericType(concreteGenericTRequest, concreteTResponse)
            : typeDefinition.MakeGenericType(concreteGenericTRequest);

        return (serviceType, openRequestHandlerImplementation.MakeGenericType(closingTypes));
    }

    private static List<Type> GetConcreteRequestTypes(Type openRequestHandlerInterface,
        Type openRequestHandlerImplementation, IEnumerable<Assembly> assembliesToScan,
        CancellationToken cancellationToken)
    {
        //request generic type constraints       
        var constraintsForEachParameter = openRequestHandlerImplementation
            .GetGenericArguments()
            .Select(x => x.GetGenericParameterConstraints())
            .ToList();

        var typesThatCanCloseForEachParameter = constraintsForEachParameter
            .Select(constraints => assembliesToScan
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type.IsClass && !type.IsAbstract &&
                    constraints.All(constraint => constraint.IsAssignableFrom(type))).ToList()
            ).ToList();

        var requestType = openRequestHandlerInterface.GenericTypeArguments.First();

        if (requestType.IsGenericParameter) return null;

        var requestGenericTypeDefinition = requestType.GetGenericTypeDefinition();

        var combinations = GenerateCombinations(requestType, typesThatCanCloseForEachParameter, 0, cancellationToken);

        return combinations.Select(types => requestGenericTypeDefinition.MakeGenericType(types.ToArray())).ToList();
    }

    private static List<List<Type>> GenerateCombinations(Type requestType, List<List<Type>> lists, int depth = 0,
        CancellationToken cancellationToken = default)
    {
        if (depth == 0)
        {
            // Initial checks
            if (MaxGenericTypeParameters > 0 && lists.Count > MaxGenericTypeParameters)
                throw new ArgumentException(
                    $"Error registering the generic type: {requestType.FullName}. The number of generic type parameters exceeds the maximum allowed ({MaxGenericTypeParameters}).");

            foreach (var list in lists)
            {
                if (MaxTypesClosing > 0 && list.Count > MaxTypesClosing)
                    throw new ArgumentException(
                        $"Error registering the generic type: {requestType.FullName}. One of the generic type parameter's count of types that can close exceeds the maximum length allowed ({MaxTypesClosing}).");
            }

            // Calculate the total number of combinations
            long totalCombinations = 1;
            foreach (var list in lists)
            {
                totalCombinations *= list.Count;
                if (MaxGenericTypeParameters > 0 && totalCombinations > MaxGenericTypeRegistrations)
                    throw new ArgumentException(
                        $"Error registering the generic type: {requestType.FullName}. The total number of generic type registrations exceeds the maximum allowed ({MaxGenericTypeRegistrations}).");
            }
        }

        if (depth >= lists.Count)
            return [[]];

        cancellationToken.ThrowIfCancellationRequested();

        var currentList = lists[depth];
        var childCombinations = GenerateCombinations(requestType, lists, depth + 1, cancellationToken);
        var combinations = new List<List<Type>>();

        foreach (var item in currentList)
        {
            foreach (var childCombination in childCombinations)
            {
                var currentCombination = new List<Type> { item };
                currentCombination.AddRange(childCombination);
                combinations.Add(currentCombination);
            }
        }

        return combinations;
    }

    private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
    {
        while (true)
        {
            if (handlerType == null || handlerInterface == null) return false;
            if (handlerType.IsInterface)
                return handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments);
            handlerType = handlerType.GetInterface(handlerInterface.Name);
        }
    }
}