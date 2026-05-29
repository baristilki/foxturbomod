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
    public bool EnableNetworkOptimization { get; set; } = true;
    public bool EnableGameDvrTweak { get; set; } = true;
    public bool EnableProcessPriority { get; set; } = true;
    public bool EnableFpsMonitor { get; set; } = true;
    public bool EnableVisualEffects { get; set; } = true;
    public bool EnableCpuParking { get; set; } = true;

    public bool StartMinimized { get; set; } = false;
    public bool AutoActivateOnGameStart { get; set; } = true;

    public Dictionary<string, GameProfile> GameProfiles { get; set; } = new();
}
