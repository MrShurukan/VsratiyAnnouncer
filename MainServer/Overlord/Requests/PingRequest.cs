using MediatR;
using Overlord.Models;

namespace Overlord.Requests;

public class PingRequest(Guid id) 
    : IRequest<Agent>
{
    public Guid Id { get; set; } = id;
}

public class PingRequestHandler(ApplicationContext context) 
    : IRequestHandler<PingRequest, Agent>
{
    public async Task<Agent> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        var agent = await context.FindAgentAndMarkAsActiveAsync(request.Id, cancellationToken);
        
        return agent;
    }
}