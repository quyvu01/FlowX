using FlowX.ApplicationModels;
using FlowX.Registries;

namespace FlowX.Extensions;

public static class PipelineExtensions
{
    public static void AddSqlPipelines(this FlowXRegister flowXRegister, Action<SqlPipeline> options)
    {
        var receivedPipeline = new SqlPipeline(flowXRegister.ServiceCollection);
        options.Invoke(receivedPipeline);
    }
}