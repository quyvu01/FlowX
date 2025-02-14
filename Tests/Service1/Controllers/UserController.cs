using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service1.Contracts.Requests;

namespace Service1.Controllers;

[Route("api/[controller]/[action]")]
public sealed class UserController(IFlow sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await sender.Send(command);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] GetUserQuery query)
    {
        object request = query;
        var result = await sender.Send(request);
        return Ok();
    }
}