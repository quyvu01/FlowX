using FlowX.Abstractions;
using FlowX.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.ApplicationModels;

public sealed class SqlPipeline(IServiceCollection serviceCollection)
{
    private static readonly Type interfaceReceivedPipeline = typeof(ISqlPipelineBehavior<,>);

    public SqlPipeline OfType<TReceivedPipeline>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        OfType(typeof(TReceivedPipeline), serviceLifetime);
        return this;
    }

    // Hmmm, this one is temporary!. I think should test more case!
    public SqlPipeline OfType(Type pipelineType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var signatureInterfaceTypes = pipelineType.GetInterfaces()
            .Where(a => a.IsGenericType && a.GetGenericTypeDefinition() == interfaceReceivedPipeline)
            .ToList();
        if (signatureInterfaceTypes is not { Count: > 0 })
            throw new FlowXExceptions.PipelineIsNotSqlPipelineBehavior(pipelineType);
        if (pipelineType.IsGenericType)
        {
            if (pipelineType.ContainsGenericParameters)
            {
                var serviceDescriptor = new ServiceDescriptor(interfaceReceivedPipeline, pipelineType, serviceLifetime);
                serviceCollection.Add(serviceDescriptor);
                return this;
            }
        }

        signatureInterfaceTypes.ForEach(s =>
        {
            var serviceDescriptor = new ServiceDescriptor(s, pipelineType, serviceLifetime);
            serviceCollection.Add(serviceDescriptor);
        });

        return this;
    }
}