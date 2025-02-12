using FlowX.Abstractions;
using FlowX.Implementations;
using FlowX.Registries;
using FlowX.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlowX.Extensions;

public static class FlowXExtensions
{
    public static FlowXRegisterWrapped AddFlowX(this IServiceCollection serviceCollection,
        Action<FlowXRegister> options)
    {
        var newFlowXRegister = new FlowXRegister(serviceCollection);
        options.Invoke(newFlowXRegister);
        serviceCollection.AddTransient(typeof(FlowPipelinesImpl<,>));
        serviceCollection.AddTransient<IFlow, FlowSender>();
        serviceCollection.AddTransient(typeof(IFlowPipelineBehavior<,>), typeof(TransportPipelineImpl<,>));
        serviceCollection.TryAddTransient(typeof(IRequestHandler<,>), typeof(DefaultRequestHandler<,>));
        return new FlowXRegisterWrapped(newFlowXRegister);
    }
}