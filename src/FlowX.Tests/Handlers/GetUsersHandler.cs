using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryMany;
using FlowX.Structs;
using FlowX.Tests.Models;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;

namespace FlowX.Tests.Handlers;

public sealed class GetUsersHandler(ISqlRepository<User> sqlRepository)
    : EfQueryCollectionHandler<User, GetUsersQuery, UserResponse>(sqlRepository)
{
    protected override IQueryListFlowBuilder<User, UserResponse> BuildQueryFlow(
        IQueryListFilter<User, UserResponse> fromFlow, IRequestContext<GetUsersQuery> queryContext)
        => fromFlow
            .WithFilter(a => queryContext.Request.Ids.Contains(a.Id))
            .WithSpecialAction(a => a.Select(x => new UserResponse { Id = x.Id, Email = x.Email, Name = x.Name }))
            .WithSortFieldWhenNotSet(a => a.Name)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
}