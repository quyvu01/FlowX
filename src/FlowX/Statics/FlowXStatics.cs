using System.Reflection;
using FlowX.Abstractions;
using FlowX.ApplicationModels;

namespace FlowX.Statics;

public static class FlowXStatics
{
    public static Func<Type, bool> ModelsFilter { get; internal set; }
    internal static Assembly HandlersFromNamespaceContaining { get; set; }
    public static TimeSpan DefaultRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    internal static RetryPolicy RetryPolicy { get; set; }

    public static Assembly ModelsFromNamespaceContaining { get; internal set; }
}