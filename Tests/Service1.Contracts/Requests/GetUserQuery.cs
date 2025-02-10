using FlowX.Abstractions.RequestFlow.Queries;
using Service1.Contracts.Responses;

namespace Service1.Contracts.Requests;

public sealed record GetUserQuery(Guid Id) : IQueryOne<UserResponse>;