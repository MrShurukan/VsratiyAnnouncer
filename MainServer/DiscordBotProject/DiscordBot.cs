using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBotProject;

public class DiscordBot
{
    public static ulong? TestChannelId { get; private set; } = null;
    private DiscordSocketClient _client = new DiscordSocketClient();
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
    
    public async Task StartAsync()
    {
        _client.Log += Log;
        // _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;

        const string settingsPath = $"../{nameof(DiscordBotProject)}/discordbot.json";
        if (!File.Exists(settingsPath))
        {
            throw new FileNotFoundException($"Нет файла discordbot.json в корне проекта {nameof(DiscordBotProject)} (см. discordbot.sample.json)");
        }
        var discordToken = JsonConvert.DeserializeObject<Settings>(await File.ReadAllTextAsync(settingsPath)).Token;
        
        Console.WriteLine("Запускаю бота...");
        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "ping":
                await PingCommandHandler(command);
                break;
            case "test-remember-channel":
                await TestRememberChannelHandler(command);
                break;
            case "start":
                await StartCommandHandler(command);
                break;
            case "stop":
                await StopCommandHandler(command);
                break;
        }

    }
    private async Task TestRememberChannelHandler(SocketSlashCommand command)
    {
        TestChannelId = command.Channel.Id;
        await command.RespondAsync(text: $"Успешно запомнил канал {TestChannelId}", ephemeral: true);
    }
    private async Task StartCommandHandler(SocketSlashCommand command)
    {
        throw new NotImplementedException();
    }
    private async Task StopCommandHandler(SocketSlashCommand command)
    {
        throw new NotImplementedException();
    }

    private async Task PingCommandHandler(SocketSlashCommand command)
    {
        var user = command.User as SocketGuildUser;
        await command.RespondAsync($"Запущена команда '{command.Data.Name}'. "
                                   + $"Получено от {command.User.Username}. "
                                   + $"В канале {command.Channel.Name}. "
                                   + $"Пользователь {(user?.VoiceState != null ? $"В голосовом канале ({user.VoiceState})" : "не в голосовом канале")}.");
    }

    public async Task CreateSlashCommands(ulong guildId)
    {
        var guild = _client.GetGuild(guildId);

        var commands = new List<SlashCommandBuilder>
        {
            // ping
            new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("Тестовая команда для проверки бота"),
            // test-remember-channel
            new SlashCommandBuilder()
                .WithName("test-remember-channel")
                .WithDescription("[Админ] Данная команда нужна для проверки связки между Overlord и DiscordBot"),
        };

        try
        {
            foreach (var command in commands)
            {
                await guild.CreateApplicationCommandAsync(command.Build());
            }
        }
        catch(HttpException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine("Exception occured when creating guild commands");
            Console.WriteLine(json);

            throw;
        }
    }
    public async Task SendTestMessageToMemorizedChannel(string customMessage)
    {
        if (TestChannelId is null) 
            throw new Exception("Нет канала для отправки (нужна команда 'test-remember-channel')");

        var channel = await _client.GetChannelAsync((ulong)TestChannelId);
        if (channel is null)
            throw new Exception($"Не удалось найти канал с id {TestChannelId}");

        if (channel is not ITextChannel textChannel)
            throw new Exception($"Это не текстовый канал ({TestChannelId})!");

        await textChannel.SendMessageAsync($"ХА! Лови сообщение от админа: {customMessage}");
    }
}