using System.IO;
using System.Text.Json;
using TurboMode.Models;

namespace TurboMode.Services;

/// <summary>
/// AppSettings'i %LOCALAPPDATA%\FoxMod\settings.json'da tutar.
/// </summary>
public static class ProfileStore
{
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FoxMod");
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "settings.json");
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return new AppSettings();
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
        }
        catch { return new AppSettings(); }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(settings, Options));
        }
        catch { }
    }
}
