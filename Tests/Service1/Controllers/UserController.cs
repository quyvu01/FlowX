using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service1.Contracts.Requests;

namespace Service1.Controllers;

[Route("api/[controller]/[action]")]
public sealed class UserController(IFlow sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        object request = query;
        var result = await sender.Send(request, cancellationToken);
        return Ok();
    }
}