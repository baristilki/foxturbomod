using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using TurboMode.Models;
using TurboMode.Native;

namespace TurboMode.Services;

/// <summary>
/// Steam, Epic, Riot ve genel Windows kayıt defterinden yüklü oyunları tespit eder.
/// Tamamen yerel — internet kullanmaz. Sadece dosya sistemi + registry okur.
/// </summary>
public sealed class GameLibraryScanner
{
    /// <summary>
    /// Yüklü oyunları (en olası olanları) liste olarak döner. Sıralama: platform + isim.
    /// </summary>
    public List<InstalledGame> Scan()
    {
        var games = new List<InstalledGame>();
        try { games.AddRange(ScanSteam()); } catch { }
        try { games.AddRange(ScanEpic()); } catch { }
        try { games.AddRange(ScanRiot()); } catch { }

        // Tekrarları process adı ile elemine et (Steam + Epic aynı oyunu listeleyebilir).
        var unique = games
            .GroupBy(g => g.ProcessName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(g => g.Platform)
            .ThenBy(g => g.DisplayName)
            .ToList();

        // Her oyun için ikon çıkar.
        foreach (var g in unique)
            g.Icon = IconExtractor.Extract(g.ExecutablePath);

        return unique;
    }

    // ─────── STEAM ─────────────────────────────────────────────────────

    private static IEnumerable<InstalledGame> ScanSteam()
    {
        var steamPath = GetSteamPath();
        if (string.IsNullOrEmpty(steamPath)) yield break;

        // libraryfolders.vdf — Steam'in oyun kütüphane yollarını tutar.
        var libVdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libVdf)) yield break;

        var libText = File.ReadAllText(libVdf);
        // "path"    "D:\\SteamLibrary" satırlarını yakalar.
        var pathMatches = Regex.Matches(libText, @"""path""\s+""([^""]+)""");
        var libraries = pathMatches.Select(m => m.Groups[1].Value.Replace(@"\\", @"\")).ToList();
        if (libraries.Count == 0) libraries.Add(steamPath);

        foreach (var lib in libraries)
        {
            var manifestDir = Path.Combine(lib, "steamapps");
            if (!Directory.Exists(manifestDir)) continue;

            foreach (var manifest in Directory.GetFiles(manifestDir, "appmanifest_*.acf"))
            {
                InstalledGame? g = null;
                try
                {
                    var text = File.ReadAllText(manifest);
                    var name = Regex.Match(text, @"""name""\s+""([^""]+)""").Groups[1].Value;
                    var installdir = Regex.Match(text, @"""installdir""\s+""([^""]+)""").Groups[1].Value;
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(installdir)) continue;

                    var gameDir = Path.Combine(lib, "steamapps", "common", installdir);
                    if (!Directory.Exists(gameDir)) continue;

                    // Steam binary'leri redistributables, launcher vs. içerebilir; en büyük .exe genelde oyun.
                    var exe = PickLikelyExecutable(gameDir);
                    if (exe == null) continue;

                    g = new InstalledGame
                    {
                        DisplayName = name,
                        ExecutablePath = exe,
                        ProcessName = Path.GetFileNameWithoutExtension(exe),
                        Platform = "Steam",
                    };
                }
                catch { }
                if (g != null) yield return g;
            }
        }
    }

    private static string? GetSteamPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            var path = key?.GetValue("SteamPath") as string;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path)) return path;
        }
        catch { }
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            var path = key?.GetValue("InstallPath") as string;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path)) return path;
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Bir oyun klasöründe muhtemel ana exe'yi bul.
    /// Heuristik: en üst seviyedeki en büyük .exe, "redist"/"vcredist"/"setup"/"unins" içermeyen.
    /// </summary>
    private static string? PickLikelyExecutable(string gameDir)
    {
        try
        {
            var candidates = new DirectoryInfo(gameDir)
                .EnumerateFiles("*.exe", SearchOption.AllDirectories)
                .Where(f => !LooksLikeJunk(f.Name))
                .Where(f => f.Length > 1_000_000) // 1MB altı genelde launcher/yardımcı
                .OrderByDescending(f => f.Length)
                .Take(3)
                .ToList();
            return candidates.FirstOrDefault()?.FullName;
        }
        catch { return null; }
    }

    private static bool LooksLikeJunk(string name)
    {
        name = name.ToLowerInvariant();
        return name.Contains("redist") || name.Contains("vcredist") ||
               name.Contains("setup") || name.Contains("unins") ||
               name.Contains("crashreport") || name.Contains("crashhandler") ||
               name.Contains("dotnet") || name.Contains("directx") ||
               name.Contains("eac") || name.Contains("anticheat") ||
               name.Contains("battleye") || name.StartsWith("be_");
    }

    // ─────── EPIC GAMES ────────────────────────────────────────────────

    private static IEnumerable<InstalledGame> ScanEpic()
    {
        var manifestDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests");
        if (!Directory.Exists(manifestDir)) yield break;

        foreach (var manifest in Directory.GetFiles(manifestDir, "*.item"))
        {
            InstalledGame? g = null;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(manifest));
                var root = doc.RootElement;
                var name = root.TryGetProperty("DisplayName", out var n) ? n.GetString() : null;
                var dir = root.TryGetProperty("InstallLocation", out var d) ? d.GetString() : null;
                var exe = root.TryGetProperty("LaunchExecutable", out var e) ? e.GetString() : null;
                if (name == null || dir == null || exe == null) continue;

                var fullExe = Path.Combine(dir, exe);
                if (!File.Exists(fullExe)) continue;

                g = new InstalledGame
                {
                    DisplayName = name,
                    ExecutablePath = fullExe,
                    ProcessName = Path.GetFileNameWithoutExtension(exe),
                    Platform = "Epic",
                };
            }
            catch { }
            if (g != null) yield return g;
        }
    }

    // ─────── RIOT GAMES ────────────────────────────────────────────────

    private static IEnumerable<InstalledGame> ScanRiot()
    {
        // Riot Client kurulu mu? Eğer evetse League/Valorant'ı arıyoruz.
        var riotInstalls = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Riot Games", "RiotClientInstalls.json");

        // Bilinen kurulum yolları (Riot launcher zaten League/Valorant'ı kendi launcher'ından açar;
        // bizim açımızdan önemli olan oyunun process adını yakalamak).
        var candidates = new (string Display, string ProcessName, string[] Paths)[]
        {
            ("League of Legends Launcher", "LeagueClient", new[]
            {
                @"C:\Riot Games\League of Legends\LeagueClient.exe",
                @"D:\Riot Games\League of Legends\LeagueClient.exe",
            }),
            ("Valorant", "VALORANT-Win64-Shipping", new[]
            {
                @"C:\Riot Games\VALORANT\live\VALORANT.exe",
                @"D:\Riot Games\VALORANT\live\VALORANT.exe",
            }),
        };

        foreach (var c in candidates)
        {
            var path = c.Paths.FirstOrDefault(File.Exists);
            if (path == null) continue;
            yield return new InstalledGame
            {
                DisplayName = c.Display,
                ExecutablePath = path,
                ProcessName = c.ProcessName,
                Platform = "Riot",
            };
        }

        // RiotClientInstalls.json'dan da tara
        if (File.Exists(riotInstalls))
        {
            InstalledGame? extra = null;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(riotInstalls));
                if (doc.RootElement.TryGetProperty("associated_client", out var clients))
                {
                    foreach (var p in clients.EnumerateObject())
                    {
                        var path = p.Value.GetString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            extra = new InstalledGame
                            {
                                DisplayName = "Riot Client",
                                ExecutablePath = path,
                                ProcessName = Path.GetFileNameWithoutExtension(path),
                                Platform = "Riot",
                            };
                            break;
                        }
                    }
                }
            }
            catch { }
            if (extra != null) yield return extra;
        }
    }
}
