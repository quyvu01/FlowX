using FlowX.Abstractions;
using FlowX.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.ApplicationModels;

public sealed class FlowPipeline(IServiceCollection serviceCollection)
{
    private static readonly Type InterfaceReceivedPipeline = typeof(IFlowPipelineBehavior<,>);

    public FlowPipeline OfType<TReceivedPipeline>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        OfType(typeof(TReceivedPipeline), serviceLifetime);
        return this;
    }

    // Hmmm, this one is temporary!. I think should test more case!
    public FlowPipeline OfType(Type pipelineType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var signatureInterfaceTypes = pipelineType.GetInterfaces()
            .Where(a => a.IsGenericType && a.GetGenericTypeDefinition() == InterfaceReceivedPipeline)
            .ToList();
        if (signatureInterfaceTypes is not { Count: > 0 })
            throw new FlowXExceptions.PipelineIsNotPipelineBehavior(pipelineType);
        if (pipelineType.IsGenericType)
        {
            if (pipelineType.ContainsGenericParameters)
            {
                serviceCollection.Add(new ServiceDescriptor(InterfaceReceivedPipeline, pipelineType, serviceLifetime));
                return this;
            }
        }

        signatureInterfaceTypes.ForEach(s =>
            serviceCollection.Add(new ServiceDescriptor(s, pipelineType, serviceLifetime)));

        return this;
    }
}