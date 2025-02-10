using FlowX.Abstractions.RequestFlow.Commands;

namespace Service2.Contracts.Requests;

public sealed record CreateProvinceCommand(string Name) : ICommandVoid;