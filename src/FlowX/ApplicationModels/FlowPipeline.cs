using FlowX.Abstractions;
using FlowX.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.ApplicationModels;

public sealed class FlowPipeline(IServiceCollection serviceCollection)
{
    private static readonly Type InterfaceReceivedPipeline = typeof(IPipelineBehavior<,>);
    private static readonly Type InterfaceReceivedVoidPipeline = typeof(IPipelineBehavior<>);

    public FlowPipeline OfType<TReceivedPipeline>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        OfType(typeof(TReceivedPipeline), serviceLifetime);
        return this;
    }

    // Hmmm, this one is temporary!. I think should test more case!
    public FlowPipeline OfType(Type pipelineType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var signatureInterfaceTypes = pipelineType.GetInterfaces()
            .Where(a => a.IsGenericType &&
                        (a.GetGenericTypeDefinition() == InterfaceReceivedPipeline ||
                         a.GetGenericTypeDefinition() == InterfaceReceivedVoidPipeline))
            .ToList();
        if (signatureInterfaceTypes is not { Count: > 0 })
            throw new FlowXExceptions.PipelineIsNotPipelineBehavior(pipelineType);
        if (pipelineType.IsGenericType)
        {
            if (pipelineType.ContainsGenericParameters)
            {
                var hasResultPipeline = pipelineType.GetInterfaces()
                    .Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == InterfaceReceivedPipeline);
                var hasVoidPipeline = pipelineType.GetInterfaces()
                    .Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == InterfaceReceivedVoidPipeline);
                if (hasResultPipeline)
                    serviceCollection.Add(new ServiceDescriptor(InterfaceReceivedPipeline, pipelineType,
                        serviceLifetime));
                if (hasVoidPipeline)
                    serviceCollection.Add(new ServiceDescriptor(InterfaceReceivedVoidPipeline, pipelineType,
                        serviceLifetime));
                return this;
            }
        }

        signatureInterfaceTypes.ForEach(s =>
            serviceCollection.Add(new ServiceDescriptor(s, pipelineType, serviceLifetime)));

        return this;
    }
}