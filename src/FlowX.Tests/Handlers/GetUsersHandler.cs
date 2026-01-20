using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;
using FlowX.Tests.Models;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;

namespace FlowX.Tests.Handlers;

public sealed class GetUsersHandler : EfQueryCollectionHandler<User, GetUsersQuery, UserResponse>
{
    protected override IQueryListFlowBuilder<User, UserResponse> BuildQueryFlow(
        IQueryListFilter<User, UserResponse> fromFlow, IRequestContext<GetUsersQuery> queryContext)
        => fromFlow
            .WithFilter(a => queryContext.Request.Ids.Contains(a.Id))
            .WithSpecialAction(a => a.Select(x => new UserResponse { Id = x.Id, Email = x.Email, Name = x.Name }))
            .WithDefaultSortFields(Asc(x => x.Name));
}