using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;
using FlowX.Structs;
using Service2.Contracts.Requests;
using Service2.Contracts.Responses;
using Service2.Models;

namespace Service2.Handlers;

public sealed class GetProvincesHandler(ISqlRepository<Province> sqlRepository)
    : EfQueryPaginationHandler<Province, GetProvincesQuery, ProvinceResponse>(sqlRepository)
{
    protected override IQueryListFlowBuilder<Province, ProvinceResponse> BuildQueryFlow(
        IQueryListFilter<Province, ProvinceResponse> fromFlow, IRequestContext<GetProvincesQuery> queryContext)
        => fromFlow
            .WithFilter(null)
            .WithSpecialAction(a => a.Select(x => new ProvinceResponse { Id = x.Id, Name = x.Name }))
            .WithSortFieldWhenNotSet(x => x.Name)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
}