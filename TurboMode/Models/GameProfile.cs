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

    public bool StartMinimized { get; set; } = true;  // Windows başlangıcında tepsiye in
    public bool AutoActivateOnGameStart { get; set; } = true;

    /// <summary>Kapatma davranışı: "Ask" (sor), "Tray" (her zaman tepsiye), "Exit" (her zaman çık).</summary>
    public string CloseAction { get; set; } = "Ask";

    /// <summary>Tema: "Fox" (turuncu), "Cyber" (mavi), "Razer" (yeşil).</summary>
    public string Theme { get; set; } = "Fox";

    /// <summary>Global hotkey: Ctrl+Alt+T (default). Boş bırakırsan hotkey yok.</summary>
    public bool EnableHotkey { get; set; } = true;

    /// <summary>Format: "Ctrl+Alt+T" gibi</summary>
    public string HotkeyTurbo { get; set; } = "Ctrl+Alt+T";
    public string HotkeyOverlay { get; set; } = "Ctrl+Alt+O";
    public string HotkeyOverlayLock { get; set; } = "Ctrl+Alt+L";

    /// <summary>İlk açılış turu görüldü mü?</summary>
    public bool HasSeenOnboarding { get; set; } = false;

    /// <summary>Overlay konumu — kullanıcı taşırsa kaydedilir, açılışta yüklenir.</summary>
    public double OverlayLeft { get; set; } = 20;
    public double OverlayTop { get; set; } = 20;
    public bool OverlayLocked { get; set; } = false;
    public bool OverlayShowFps { get; set; } = true;
    public bool OverlayShowCpu { get; set; } = true;
    public bool OverlayShowGpu { get; set; } = true;
    public bool OverlayShowRam { get; set; } = true;
    public string OverlayOrientation { get; set; } = "Horizontal"; // "Horizontal", "Vertical", "Square"

    public Dictionary<string, GameProfile> GameProfiles { get; set; } = new();
}
