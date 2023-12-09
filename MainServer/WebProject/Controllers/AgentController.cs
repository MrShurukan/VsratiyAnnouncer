using MediatR;
using Microsoft.AspNetCore.Mvc;
using Overlord.Models;
using Overlord.Requests;

namespace WebProject.Controllers;

[ApiController]
[Route("/agent/{agentId}/")]
public class AgentController(IMediator mediator)
    : Controller
{
    [HttpPost("Register")]
    [ProducesResponseType<Agent>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(Guid agentId, CancellationToken token)
    {
        var agent = await mediator.Send(new RegisterRequest(agentId), token);
        return Ok(agent);
    }
    
    [HttpPost("SignOff")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SignOff(Guid agentId, CancellationToken token)
    {
        var agent = await mediator.Send(new SignOffRequest(agentId), token);
        return Ok("Успешно выполнен выход");
    }
}