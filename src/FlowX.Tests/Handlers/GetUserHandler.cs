using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.EntityFrameworkCore.Abstractions;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;
using FlowX.Errors;
using FlowX.Tests.Models;
using FlowX.Tests.Requests;
using FlowX.Tests.Responses;

namespace FlowX.Tests.Handlers;

public sealed class GetUserHandler(ISqlRepository<User> sqlRepository)
    : EfQueryOneHandler<User, GetUserQuery, UserResponse>(sqlRepository)
{
    protected override IQueryOneFlowBuilder<User, UserResponse> BuildQueryFlow(
        IQueryOneFilter<User, UserResponse> fromFlow, IRequestContext<GetUserQuery> queryContext)
        => fromFlow
            .WithFilter(a => a.Id == queryContext.Request.Id)
            .WithSpecialAction(a => a)
            .WithMap(u => new UserResponse { Id = u.Id, Email = u.Email, Name = u.Name })
            .WithErrorIfNull(new Error());
}