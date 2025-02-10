using FlowX.Abstractions.RequestFlow.Queries;
using Service2.Contracts.Responses;

namespace Service2.Contracts.Requests;

public sealed record GetProvinceQuery(Guid Id) : IQueryOne<ProvinceResponse>;