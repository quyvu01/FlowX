using FlowX.ApplicationModels;
using FlowX.Registries;

namespace FlowX.Extensions;

public static class PipelineExtensions
{
    public static void AddPipelines(this FlowXRegister flowXRegister, Action<FlowPipeline> options)
    {
        var receivedPipeline = new FlowPipeline(flowXRegister.ServiceCollection);
        options.Invoke(receivedPipeline);
    }
}