using FlowX.Structs;

namespace FlowX.Abstractions.RequestFlow.Queries;

public record GetManyQuery(int? PageSize, int? PageIndex, string SortedFieldName, SortedDirection? SortedDirection)
{
    public GetManyQuery() : this(null, null, null, null)
    {
    }

    public int? Skip() => (PageIndex, PageSize) switch
    {
        ({ } pageIndex, { } pageSize) => (pageIndex - 1) * pageSize,
        _ => null
    };

    public int? Take() => PageSize;
}