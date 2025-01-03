using FlowX.Abstractions.RequestFlow.Queries;
using FlowX.Tests.Responses;

namespace FlowX.Tests.Requests;

public sealed record GetUserQuery(string Id) : IQueryOne<UserResponse>;