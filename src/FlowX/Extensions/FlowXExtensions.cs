using FlowX.Abstractions;
using FlowX.Implementations;
using FlowX.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace FlowX.Extensions;

public static class FlowXExtensions
{
    public static FlowXRegister AddFlowX(this IServiceCollection serviceCollection, Action<FlowXRegister> options)
    {
        var newFlowXRegister = new FlowXRegister(serviceCollection);
        options.Invoke(newFlowXRegister);
        serviceCollection.AddTransient(typeof(FlowPipelinesImpl<,>));
        serviceCollection.AddTransient<IFlowXSender, FlowXSender>();
        serviceCollection.AddTransient(typeof(IFlowPipelineBehavior<,>), typeof(TransportPipelineImpl<,>));
        return newFlowXRegister;
    }
}