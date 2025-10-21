namespace FlowX.Cached;

public static class FlowXCached
{
    public static IReadOnlyDictionary<Type, Type> RequestMapResponse => InternalRequestMapResponse.Value;
    internal static readonly Lazy<Dictionary<Type, Type>> InternalRequestMapResponse = new(() => []);
}