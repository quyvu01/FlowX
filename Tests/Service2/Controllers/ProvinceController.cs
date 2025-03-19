using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service2.Contracts.Requests;

namespace Service2.Controllers;

[Route("api/[controller]/[action]")]
public sealed class ProvinceController(IFlow sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProvince([FromBody] CreateProvinceCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetProvince([FromQuery] GetProvinceQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }
}