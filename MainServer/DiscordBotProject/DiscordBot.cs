using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DiscordBotProject;

public class DiscordBot
{
    private readonly DiscordSocketClient _client = new();
    private InteractionService _interactionService = default!;

    private readonly IServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task StartAsync()
    {
        _client.Log += Log;
        // _client.Ready += ClientReady;
        // _client.SlashCommandExecuted += SlashCommandHandler;

        _interactionService = new InteractionService(_client.Rest);
        try
        {
            await _interactionService.AddModulesAsync(typeof(MainInteractionModule).Assembly, _serviceProvider);
        }
        catch (Exception e)
        {
            Console.WriteLine("\n\nERROR WHILE ADDING MODULES: " + e.Message);
            throw;
        }

        _client.InteractionCreated += async (x) =>
        {
            var ctx = new SocketInteractionContext(_client, x);
            await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
        };

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

    public async Task CreateSlashCommandsAsync(ulong guildId)
    {
        await _interactionService.RegisterCommandsToGuildAsync(guildId);
    }
    public async Task SendTestMessageToMemorizedChannelAsync(string customMessage)
    {
        if (MainInteractionModule.TestChannelId is null)
            throw new Exception("Нет канала для отправки (нужна команда 'test-remember-channel')");

        var channel = await _client.GetChannelAsync((ulong)MainInteractionModule.TestChannelId);
        if (channel is null)
            throw new Exception($"Не удалось найти канал с id {MainInteractionModule.TestChannelId}");

        if (channel is not ITextChannel textChannel)
            throw new Exception($"Это не текстовый канал ({MainInteractionModule.TestChannelId})!");

        await textChannel.SendMessageAsync($"ХА! Лови сообщение от админа: {customMessage}");
    }
}