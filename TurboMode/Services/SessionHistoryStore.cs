using System.IO;
using System.Text.Json;
using TurboMode.Models;

namespace TurboMode.Services;

/// <summary>
/// Her oyun seansını %LOCALAPPDATA%\FoxMod\history.json dosyasına ekler.
/// Maksimum 200 seansı tutar (oldest first kaldırılır).
/// </summary>
public static class SessionHistoryStore
{
    private const int MaxRecords = 200;
    private static readonly string Path =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FoxMod", "history.json");
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static List<SessionRecord> Load()
    {
        try
        {
            if (!File.Exists(Path)) return new();
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<List<SessionRecord>>(json, Options) ?? new();
        }
        catch { return new(); }
    }

    public static void Append(SessionRecord record)
    {
        try
        {
            var list = Load();
            list.Add(record);
            if (list.Count > MaxRecords)
                list.RemoveRange(0, list.Count - MaxRecords);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, JsonSerializer.Serialize(list, Options));
        }
        catch { }
    }

    /// <summary>Son N seans (en yeni başta).</summary>
    public static List<SessionRecord> Recent(int n = 20)
    {
        var all = Load();
        all.Reverse();
        return all.Take(n).ToList();
    }

    /// <summary>Bu haftanın özeti.</summary>
    public static (int SessionCount, double AvgFpsGain, long TotalRamFreedMb, TimeSpan TotalTime) WeeklySummary()
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var thisWeek = Load().Where(s => s.StartedUtc >= weekAgo).ToList();
        if (thisWeek.Count == 0) return (0, 0, 0, TimeSpan.Zero);
        var avgGain = thisWeek.Where(s => s.FpsGainPercent > 0).Select(s => s.FpsGainPercent).DefaultIfEmpty(0).Average();
        var totalRam = thisWeek.Sum(s => s.RamFreedMb);
        var totalTime = TimeSpan.FromTicks(thisWeek.Sum(s => s.Duration.Ticks));
        return (thisWeek.Count, avgGain, totalRam, totalTime);
    }
}
