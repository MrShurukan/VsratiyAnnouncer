using Newtonsoft.Json;

namespace DiscordBotProject;

public class Settings
{
    public string Token { get; set; }
    public bool LeftAloneAutoDisconnectFromVoice { get; set; }
    public int LeftAloneAutoDisconnectMinutes { get; set; }

    private static readonly object WriteToFileLock = new();
    private const string SettingsPath = $"../{nameof(DiscordBotProject)}/discordbot.json";
    public void WriteToFile()
    {
        lock (WriteToFileLock)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsPath, str);
        }
    }
    public static async Task<Settings> GetFromFileAsync()
    {
        if (!File.Exists(SettingsPath))
        {
            throw new FileNotFoundException($"Нет файла discordbot.json в корне проекта {nameof(DiscordBotProject)} (см. discordbot.sample.json)");
        }

        return JsonConvert.DeserializeObject<Settings>(await File.ReadAllTextAsync(SettingsPath))!;
    }

    public static Settings GetFromFile()
    {
        if (!File.Exists(SettingsPath))
        {
            throw new FileNotFoundException($"Нет файла discordbot.json в корне проекта {nameof(DiscordBotProject)} (см. discordbot.sample.json)");
        }

        return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath))!;
    }
}