using FlowX.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Service1.Contracts.Requests;

namespace Service1.Controllers;

[Route("api/[controller]/[action]")]
public sealed class UserController(IMediator sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAsObject([FromQuery] GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        object queryAsObject = query;
        var result = await sender.Send(queryAsObject, cancellationToken);
        return Ok(result);
    }
}