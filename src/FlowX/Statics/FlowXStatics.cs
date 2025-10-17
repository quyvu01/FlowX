using System.Reflection;
using FlowX.Abstractions;

namespace FlowX.Statics;

public static class FlowXStatics
{
    public static Func<Type, bool> ModelsFilter { get; internal set; }
    internal static Assembly HandlersFromNamespaceContaining { get; set; }
    public static TimeSpan DefaultRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public static Assembly ModelsFromNamespaceContaining { get; internal set; }

    public static Lazy<IReadOnlyCollection<Type>> HandlerTypesLazy => new(() =>
    [
        ..HandlersFromNamespaceContaining?.ExportedTypes
            .Where(x => typeof(IRequestHandlerBase).IsAssignableFrom(x) &&
                        x is { IsInterface: false, IsAbstract: false }) ?? []
    ]);
}