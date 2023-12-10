using DiscordBotProject;
using Overlord;

namespace WebProject.Services;

public class RemoveInactiveAgentsService(IServiceProvider serviceProvider) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        ConsoleWriter.WriteInfoLn("Запускаю сервис по удалению неактивных агентов");

        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetService<ApplicationContext>()!;
        while (!cancellationToken.IsCancellationRequested)
        {
            await ApplicationLogic.RemoveInactiveAgents(context, cancellationToken);

            context.ChangeTracker.Clear();
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }
}