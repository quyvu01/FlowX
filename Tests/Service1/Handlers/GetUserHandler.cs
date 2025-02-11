using FlowX.Abstractions;
using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;
using FlowX.EntityFrameworkCore.RequestHandlers.Queries.QueryOne;
using FlowX.Errors;
using Service1.Contracts.Requests;
using Service1.Contracts.Responses;
using Service1.Models;

namespace Service1.Handlers;

public sealed class GetUserHandler(ISqlRepository<User> sqlRepository)
    : EfQueryOneHandler<User, GetUserQuery, UserResponse>(sqlRepository)
{
    protected override IQueryOneFlowBuilder<User, UserResponse> BuildQueryFlow(
        IQueryOneFilter<User, UserResponse> fromFlow, IRequestXContext<GetUserQuery> queryXContext)
        => fromFlow
            .WithFilter(a => a.Id == queryXContext.Request.Id)
            .WithSpecialAction(a => a.Select(u => new UserResponse { Id = u.Id, Name = u.Name }))
            .WithErrorIfNull(new Error { Code = "NotFound", Messages = ["User was not found!"] });
}