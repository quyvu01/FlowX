using System.Reflection;

namespace FlowX.Statics;

public static class FlowXStatics
{
    internal static Assembly HandlersFromNamespaceContaining { get; set; }
    public static TimeSpan DefaultRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}