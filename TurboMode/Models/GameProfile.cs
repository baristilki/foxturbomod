namespace TurboMode.Models;

public sealed class GameProfile
{
    public string ProcessName { get; set; } = "";
    public List<string> ExtraWhitelist { get; set; } = new();
    public List<string> ExtraTargets { get; set; } = new();
}

public sealed class AppSettings
{
    public List<string> CustomWhitelist { get; set; } = new();
    public List<string> CustomTargets { get; set; } = new();

    public bool EnableMemoryCleanup { get; set; } = true;
    public bool EnablePowerPlan { get; set; } = true;
    // Default false — bazı ev router'larında DSCP'li paketleri yanlış yönlendiriyor (CoD latency artışı).
    // Kullanıcı isterse settings.json'dan açabilir.
    public bool EnableNetworkOptimization { get; set; } = false;
    public bool EnableGameDvrTweak { get; set; } = true;
    public bool EnableProcessPriority { get; set; } = true;
    public bool EnableFpsMonitor { get; set; } = true;
    public bool EnableVisualEffects { get; set; } = true;
    public bool EnableCpuParking { get; set; } = true;
    public bool EnableSystemResponsiveness { get; set; } = true;
    public bool EnableMouseTweak { get; set; } = true;

    public bool StartMinimized { get; set; } = false;
    public bool AutoActivateOnGameStart { get; set; } = true;

    /// <summary>Kapatma davranışı: "Ask" (sor), "Tray" (her zaman tepsiye), "Exit" (her zaman çık).</summary>
    public string CloseAction { get; set; } = "Ask";

    public Dictionary<string, GameProfile> GameProfiles { get; set; } = new();
}
