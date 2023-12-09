using MediatR;
using Overlord.Models;
using Overlord.StatusExceptions;

namespace Overlord.Requests;

public class RegisterRequest(Guid id) : IRequest<Agent>
{
    public Guid Id { get; set; } = id;
}

public class RegisterRequestHandler(ApplicationContext context)
    : IRequestHandler<RegisterRequest, Agent>
{
    public async Task<Agent> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var agent = await context.Agents.FindRequiredAsync(request.Id, cancellationToken);
        if (agent.IsActive && ((DateTime.UtcNow - agent.LastCommandTime) < TimeSpan.FromMinutes(2)))
        {
            throw new ImATeapot418Exception("Такой агент уже подключен");
        }

        agent.IsActive = true;
        agent.LastCommandTime = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return agent;
    }
}