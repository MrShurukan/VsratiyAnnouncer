using System.Collections.Concurrent;
using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordBotProject;

public class MainInteractionModule : InteractionModuleBase
{
    public static ulong? TestChannelId { get; private set; } = null;
    public static IAudioClient? AudioClient { get; private set; } = null;
    private static AudioOutStream? _audioOutStream = null;
    
    [SlashCommand("echo", "Тестовая команда для проверки бота")]
    public async Task Echo(string input)
    {
        await RespondAsync(input);
    }

    [SlashCommand("test-remember-channel", "[Админ] Данная команда нужна для проверки связки между WebProject и DiscordBot")]
    public async Task TestRememberChannel(ITextChannel channel)
    {
        TestChannelId = channel.Id;
        await RespondAsync(text: $"Успешно запомнил канал '{TestChannelId}'", ephemeral: true);
    }
    
    [SlashCommand("start", "Да начнётся треш в голосовом канале!", runMode: RunMode.Async)]
    public async Task Start()
    {
        var user = Context.User as IGuildUser;
        if (user!.VoiceChannel == null)
        {
            await RespondAsync("Дядя, в канал-то зайди голосовой ежжи", ephemeral: true);
            return;
        }

        try
        {
            await RespondAsync("Стартуем!");
            AudioClient = await user.VoiceChannel.ConnectAsync();
        }
        catch (Exception e)
        {
            ConsoleWriter.WriteDangerLn("Error while connecting to a VC: " + e.Message);
        }
    }

    [SlashCommand("play-sound", "Играет звук из папки со звуками бота", runMode: RunMode.Async)]
    public async Task PlaySound([Summary(name: "file-name", description: "Имя файла")] string fileName)
    {
        await DeferAsync();
        
        try
        {
            await PlayAudioAsync($"Sounds/{fileName}");
        }
        catch (Exception e)
        {
            await ModifyOriginalResponseAsync(x => x.Content = $"Произошла ошибка: {e.Message}");
            return;
        }

        await ModifyOriginalResponseAsync(x => x.Content= $"Звук '{fileName}' проигран");
    }
    
    public static async Task PlayAudioAsync(string path)
    {
        if (AudioClient is null)
            throw new Exception("Не подключен к какому-либо каналу");

        await SendAsync(AudioClient, path);
    }
    
    private static async Task SendAsync(IAudioClient client, string path)
    {
        // Create FFmpeg using the previous example
        using (var ffmpeg = CreateStream(path))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        {
            _audioOutStream ??= client.CreatePCMStream(AudioApplication.Mixed);
            try
            {
                await output.CopyToAsync(_audioOutStream);
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteDangerLn($"Error while sending audio: {e.Message}");
                throw;
            }
            finally
            {
                await _audioOutStream.FlushAsync();
            }
        }
    }
    
    private static Process CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        }) ?? throw new Exception("Не смог создать Process (Process.Start вернул null)");
    }
    
    [SlashCommand("stop", "Хватит на сегодня интернета")]
    public async Task Stop()
    {
        // var audioClient = (Context.Channel as IGuildChannel)!.Guild.AudioClient;

        if (AudioClient != null)
        {
            // Stop the audio client if needed (e.g., if it's playing or streaming something)
            // ...

            if (_audioOutStream is not null)
            {
                await _audioOutStream.DisposeAsync();
                _audioOutStream = null;
            }

            // Disconnect the audio client
            await AudioClient.StopAsync();
            await RespondAsync("А на сегодня всё...");

            AudioClient = null;
        }
        else
        {
            await RespondAsync("Дядя, у тебя шиза, я даже к каналу не подключен никакому", ephemeral: true);
        }
    }
}