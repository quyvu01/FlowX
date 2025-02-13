using FlowX.Abstractions;

namespace Service1.Contracts.Requests;

public sealed record CreateTestCommand() : IRequest<string>;