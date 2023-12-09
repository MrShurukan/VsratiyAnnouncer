using DiscordBotProject;
using Microsoft.EntityFrameworkCore;

namespace Overlord;

public static class ApplicationLogic
{
    public static async Task RemoveInactiveAgents(ApplicationContext context, CancellationToken token = default)
    {
        var atLeastOne = false;
        foreach (var agent in await context.Agents
                     .Where(x => 
                         (DateTime.UtcNow - x.LastCommandTime) > TimeSpan.FromMinutes(2))
                     .ToListAsync(token))
        {
            agent.IsActive = false;
            agent.LastCommandTime = null;
            
            ConsoleWriter.WriteWarningLn($"Отсоединяю {agent.Id} ({agent.PersonName}) по причине неактивности");
            
            atLeastOne = true;
        }

        if (atLeastOne)
        {
            await context.SaveChangesAsync(token);
        }
    }
}