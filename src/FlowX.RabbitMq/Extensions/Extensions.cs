namespace FlowX.RabbitMq.Extensions;

internal static class Extensions
{
    internal static string GetExchangeName(this Type type) => $"FlowX-{type.Namespace}:{type.Name}";
}