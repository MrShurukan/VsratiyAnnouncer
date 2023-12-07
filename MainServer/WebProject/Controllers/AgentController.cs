using Microsoft.AspNetCore.Mvc;

namespace WebProject.Controllers;

[ApiController]
[Route("/agent/{agentId}/")]
public class AgentController : Controller
{
    [HttpGet("ping")]
    public IActionResult Ping(string agentId)
    {
        return Ok($"Pong, Mr. {agentId}!");
    }

    [HttpPost("register")]
    public IActionResult Register(string agentId)
    {
        throw new NotImplementedException();
    }
}