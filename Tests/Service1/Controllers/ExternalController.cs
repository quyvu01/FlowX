using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service2.Contracts.Requests;

namespace Service1.Controllers;

[Route("api/[controller]/[action]")]
public sealed class ExternalController(IFlow sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProvince([FromQuery] GetProvinceQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetProvinces([FromQuery] GetProvincesQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}