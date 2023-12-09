using Newtonsoft.Json;

namespace Overlord;

public class Settings
{
    public string DbConnection { get; set; } = "";
    private const string SettingsPath = $"../{nameof(Overlord)}/overlord.json";
    
    public static async Task<Settings> GetFromFileAsync()
    {
        if (!File.Exists(SettingsPath))
        {
            throw new FileNotFoundException($"Нет файла overlord.json в корне проекта {nameof(Overlord)} (см. overlord.sample.json)");
        }

        return JsonConvert.DeserializeObject<Settings>(await File.ReadAllTextAsync(SettingsPath))!;
    }

    public static Settings GetFromFile()
    {
        if (!File.Exists(SettingsPath))
        {
            throw new FileNotFoundException($"Нет файла overlord.json в корне проекта {nameof(Overlord)} (см. overlord.sample.json)");
        }

        return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath))!;
    }
}