namespace FlowX.Responses;

public sealed class CollectionResponse<T>(List<T> items) where T : class
{
    public List<T> Items { get; } = items;
    public long TotalRecord { get; } = items.Count;
}