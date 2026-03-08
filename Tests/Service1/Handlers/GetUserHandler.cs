using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;
using FlowX.Errors;
using Service1.Contracts.Requests;
using Service1.Contracts.Responses;
using Service1.Models;

namespace Service1.Handlers;

public sealed class GetUserHandler
    : EfQueryOneHandler<GetUserQuery, UserResponse>
{
    protected override IQueryOneFlowBuilder<UserResponse> BuildQueryFlow(
        IQueryOneFilter<UserResponse> fromFlow, IRequestContext<GetUserQuery> queryContext)
        => fromFlow
            .WithFilter<User>(a => a.Id == queryContext.Request.Id)
            .WithSpecialAction(a => a.Select(u => new UserResponse { Id = u.Id, Name = u.Name }))
            .WithErrorIfNull(new Error { Code = "NotFound", Messages = ["User was not found!"] });
}
