using System.Diagnostics;
using System.Management;

namespace TurboMode.Services;

/// <summary>
/// Bilinen oyun süreçlerinin başlamasını/durmasını dinler.
/// WMI process start/stop event'leri kullanır — polling yok, neredeyse sıfır CPU yükü.
/// </summary>
public sealed class GameDetector : IDisposable
{
    public event Action<string, string>? GameStarted;   // (oyun adı, process adı)
    public event Action<string>? GameStopped;           // (oyun adı)

    public string? CurrentGame { get; private set; }
    public string? CurrentGameProcess => _currentProcess;
    private string? _currentProcess;

    private ManagementEventWatcher? _startWatcher;
    private ManagementEventWatcher? _stopWatcher;

    /// <summary>
    /// "process_adi.exe" -> "Görünür isim"
    /// </summary>
    private static readonly Dictionary<string, string> KnownGames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Riot — hem launcher hem in-game süreçlerini izle ki kullanıcı launcher'ı açar açmaz tespit edelim
        ["VALORANT-Win64-Shipping.exe"] = "Valorant",
        ["VALORANT.exe"] = "Valorant",
        ["RiotClientServices.exe"] = "Riot Client",
        ["LeagueClient.exe"] = "League of Legends",
        ["LeagueClientUx.exe"] = "League of Legends",
        ["LeagueClientUxRender.exe"] = "League of Legends",
        ["LeagueofLegends.exe"] = "League of Legends",
        ["League of Legends.exe"] = "League of Legends",
        ["cs2.exe"] = "Counter-Strike 2",
        ["FortniteClient-Win64-Shipping.exe"] = "Fortnite",
        ["r5apex.exe"] = "Apex Legends",
        ["r5apex_dx12.exe"] = "Apex Legends",
        ["RainbowSix.exe"] = "Rainbow Six Siege",
        ["RainbowSix_Vulkan.exe"] = "Rainbow Six Siege",
        ["TslGame.exe"] = "PUBG",
        ["overwatch.exe"] = "Overwatch 2",
        ["dota2.exe"] = "Dota 2",
        ["RocketLeague.exe"] = "Rocket League",
        ["WorldOfTanks.exe"] = "World of Tanks",
        ["GTA5.exe"] = "GTA V",
        ["RDR2.exe"] = "Red Dead Redemption 2",
        ["EscapeFromTarkov.exe"] = "Escape from Tarkov",
        ["Minecraft.Windows.exe"] = "Minecraft",
        ["javaw.exe"] = "Minecraft (Java)", // not: false positive olabilir
        ["RustClient.exe"] = "Rust",
        ["DeadByDaylight-Win64-Shipping.exe"] = "Dead by Daylight",
        ["DyingLightGame_x64_rwdi.exe"] = "Dying Light 2",
        ["eldenring.exe"] = "Elden Ring",
        ["Cyberpunk2077.exe"] = "Cyberpunk 2077",
        ["Hogwarts Legacy.exe"] = "Hogwarts Legacy",

        // Call of Duty serisi — modern başlıklar tek "cod.exe" kullanıyor
        ["cod.exe"] = "Call of Duty",
        ["ModernWarfare.exe"] = "Call of Duty: MW (2019)",
        ["Warzone.exe"] = "Call of Duty: Warzone",
        ["BlackOpsColdWar.exe"] = "CoD: Black Ops Cold War",
        ["BlackOps3.exe"] = "CoD: Black Ops 3",
        ["BlackOps4.exe"] = "CoD: Black Ops 4",
        ["iw7_ship.exe"] = "CoD: Infinite Warfare",
        ["iw6mp64_ship.exe"] = "CoD: Ghosts",
        ["s2_mp64_ship.exe"] = "CoD: Black Ops 2",
        ["Vanguard.exe"] = "CoD: Vanguard",

        // Battlefield serisi
        ["bf2042.exe"] = "Battlefield 2042",
        ["bf1.exe"] = "Battlefield 1",
        ["bfv.exe"] = "Battlefield V",
        ["bf4.exe"] = "Battlefield 4",

        // Diğer popüler
        ["FIFA23.exe"] = "FIFA 23",
        ["FC24.exe"] = "EA FC 24",
        ["FC25.exe"] = "EA FC 25",
        ["DyingLightGame.exe"] = "Dying Light",
        ["The Finals.exe"] = "The Finals",
        ["Discovery.exe"] = "The Finals", // alternatif
        ["MarvelRivals_Launcher.exe"] = "Marvel Rivals",
        ["MarvelRivals.exe"] = "Marvel Rivals",
        ["Helldivers2.exe"] = "Helldivers 2",
        ["Palworld-Win64-Shipping.exe"] = "Palworld",
    };

    public void Start()
    {
        // Mevcut oyunu kontrol et (uygulama açılırken zaten oyun çalışıyor olabilir).
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                if (KnownGames.TryGetValue(p.ProcessName + ".exe", out var name) ||
                    KnownGames.TryGetValue(p.MainModule?.ModuleName ?? "", out name))
                {
                    CurrentGame = name;
                    _currentProcess = p.ProcessName;
                    GameStarted?.Invoke(name, p.ProcessName);
                    break;
                }
            }
            catch { }
            finally { p.Dispose(); }
        }

        // WMI event watchers
        try
        {
            var startQuery = new WqlEventQuery(
                "SELECT * FROM Win32_ProcessStartTrace");
            _startWatcher = new ManagementEventWatcher(startQuery);
            _startWatcher.EventArrived += OnProcessStarted;
            _startWatcher.Start();

            var stopQuery = new WqlEventQuery(
                "SELECT * FROM Win32_ProcessStopTrace");
            _stopWatcher = new ManagementEventWatcher(stopQuery);
            _stopWatcher.EventArrived += OnProcessStopped;
            _stopWatcher.Start();
        }
        catch
        {
            // WMI yoksa polling'e fallback — şimdilik sessizce geç. UI çalışmaya devam eder.
        }
    }

    private void OnProcessStarted(object sender, EventArrivedEventArgs e)
    {
        var name = e.NewEvent.Properties["ProcessName"]?.Value?.ToString();
        if (string.IsNullOrEmpty(name)) return;

        if (KnownGames.TryGetValue(name, out var displayName))
        {
            CurrentGame = displayName;
            _currentProcess = name;
            GameStarted?.Invoke(displayName, name);
        }
    }

    private void OnProcessStopped(object sender, EventArrivedEventArgs e)
    {
        var name = e.NewEvent.Properties["ProcessName"]?.Value?.ToString();
        if (string.IsNullOrEmpty(name)) return;

        if (string.Equals(name, _currentProcess + ".exe", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, _currentProcess, StringComparison.OrdinalIgnoreCase))
        {
            var ended = CurrentGame ?? "Bilinmiyor";
            CurrentGame = null;
            _currentProcess = null;
            GameStopped?.Invoke(ended);
        }
    }

    public void Dispose()
    {
        try { _startWatcher?.Stop(); _startWatcher?.Dispose(); } catch { }
        try { _stopWatcher?.Stop(); _stopWatcher?.Dispose(); } catch { }
    }
}
