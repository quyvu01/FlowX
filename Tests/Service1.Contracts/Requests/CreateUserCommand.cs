using FlowX.Abstractions.RequestFlow.Commands;

namespace Service1.Contracts.Requests;

public sealed record CreateUserCommand(string Name) : ICommandVoid;