using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service1.Contracts.Requests;

namespace Service1.Controllers;

[Route("api/[controller]/[action]")]
public sealed class UserController(IFlowXSender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await sender.ExecuteAsync(command);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] GetUserQuery query)
    {
        var result = await sender.ExecuteAsync(query);
        return result.Match<IActionResult>(Ok, BadRequest);
    }
}