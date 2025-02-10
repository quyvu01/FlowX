using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;
using FlowX.Errors;
using Service2.Contracts.Requests;
using Service2.Contracts.Responses;
using Service2.Models;

namespace Service2.Handlers;

public sealed class GetProvinceHandler(ISqlRepository<Province> sqlRepository)
    : EfQueryOneHandler<Province, GetProvinceQuery, ProvinceResponse>(sqlRepository)
{
    protected override IQueryOneFlowBuilder<Province, ProvinceResponse> BuildQueryFlow(
        IQueryOneFilter<Province, ProvinceResponse> fromFlow, RequestContext<GetProvinceQuery> queryContext)
        => fromFlow
            .WithFilter(a => a.Id == queryContext.Request.Id)
            .WithSpecialAction(a => a.Select(u => new ProvinceResponse { Id = u.Id, Name = u.Name }))
            .WithErrorIfNull(new Error { Code = "NotFound", Messages = ["User was not found!"] });
}