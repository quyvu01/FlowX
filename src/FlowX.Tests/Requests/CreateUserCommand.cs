using FlowX.Abstractions.RequestFlow.Commands;

namespace FlowX.Tests.Requests;

public sealed record CreateUserCommand(string Id, string Name, string Email) : ICommandVoid;