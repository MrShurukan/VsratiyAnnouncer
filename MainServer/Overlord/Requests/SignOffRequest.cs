using MediatR;

namespace Overlord.Requests;

public class SignOffRequest(Guid id) : IRequest<Unit>
{
    public Guid Id { get; set; } = id;
}

public class SingOffRequestHandler(ApplicationContext context) 
    : IRequestHandler<SignOffRequest, Unit>
{
    public async Task<Unit> Handle(SignOffRequest request, CancellationToken cancellationToken)
    {
        var agent = await context.Agents.FindRequiredAsync(request.Id, cancellationToken);

        agent.IsActive = false;
        agent.LastCommandTime = null;

        return Unit.Value;
    }
}