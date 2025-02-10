using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service2.Contracts.Requests;

namespace Service2.Controllers;

[Route("api/[controller]/[action]")]
public sealed class ProvinceController(IFlowXSender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProvince([FromBody] CreateProvinceCommand command)
    {
        var result = await sender.ExecuteAsync(command);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetProvince([FromQuery] GetProvinceQuery query)
    {
        var result = await sender.ExecuteAsync(query);
        return result.Match<IActionResult>(Ok, BadRequest);
    }
}