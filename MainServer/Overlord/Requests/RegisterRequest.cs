using DiscordBotProject;
using MediatR;
using Overlord.Models;
using Overlord.StatusExceptions;

namespace Overlord.Requests;

public class RegisterRequest(Guid id, bool force = false) : IRequest<Agent>
{
    public Guid Id { get; set; } = id;
    public bool Force { get; set; } = force;
}

public class RegisterRequestHandler(ApplicationContext context)
    : IRequestHandler<RegisterRequest, Agent>
{
    public async Task<Agent> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var agent = await context.Agents.FindRequiredAsync(request.Id, cancellationToken);
        if (agent.IsActive && ((DateTime.UtcNow - agent.LastCommandTime) < TimeSpan.FromMinutes(2)))
        {
            if (request.Force)
            {
                ConsoleWriter.WriteWarningLn($"Агент {request.Id} подключился с force=true");
            }
            else
            {
                throw new ImATeapot418Exception("Такой агент уже подключен");
            }
        }

        agent.IsActive = true;
        agent.LastCommandTime = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return agent;
    }
}