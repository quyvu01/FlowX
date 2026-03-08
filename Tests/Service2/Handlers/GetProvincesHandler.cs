using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;
using Service2.Contracts.Requests;
using Service2.Contracts.Responses;
using Service2.Models;

namespace Service2.Handlers;

public sealed class GetProvincesHandler
    : EfQueryPaginationHandler<GetProvincesQuery, ProvinceResponse>
{
    protected override IQueryListFlowBuilder<ProvinceResponse> BuildQueryFlow(
        IQueryListFilter<ProvinceResponse> fromFlow, IRequestContext<GetProvincesQuery> queryContext)
        => fromFlow
            .WithFilter<Province>(null)
            .WithSpecialAction(a => a.Select(x => new ProvinceResponse { Id = x.Id, Name = x.Name }))
            .WithDefaultSortFields(Asc<Province>(a => a.Name).ThenDescBy(x => x.Id));
}
