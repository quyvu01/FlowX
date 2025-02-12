using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service2.Contracts.Requests;

namespace Service1.Controllers;

[Route("api/[controller]/[action]")]
public sealed class ExternalController(IFlow sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProvince([FromQuery] GetProvinceQuery query)
    {
        var result = await sender.Send(query);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetProvinces([FromQuery] GetProvincesQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
}