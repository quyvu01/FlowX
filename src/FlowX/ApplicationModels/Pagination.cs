namespace FlowX.ApplicationModels;

public sealed class Pagination<T>
{
    public IEnumerable<T> Items { get; set; }
    public long TotalRecord { get; set; }
}