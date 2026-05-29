using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// Sistemin "gaming-ready" olup olmadığını kontrol eder. Read-only.
/// HAGS (Hardware Accelerated GPU Scheduling) ve DirectStorage durumunu okur.
/// </summary>
public static class SystemStateChecker
{
    public record SystemState(
        bool HagsEnabled,
        bool HagsSupported,
        bool DirectStorageSupported,
        bool IsWindows11,
        string WindowsVersion);

    public static SystemState Check()
    {
        bool hagsEnabled = false;
        bool hagsSupported = false;
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers");
            // HwSchMode: 2 = on, 1 = off, yoksa desteklenmiyor
            var v = k?.GetValue("HwSchMode");
            if (v is int mode)
            {
                hagsSupported = true;
                hagsEnabled = mode == 2;
            }
        }
        catch { }

        bool isWin11 = false;
        string winVersion = "Unknown";
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var build = k?.GetValue("CurrentBuild")?.ToString();
            var product = k?.GetValue("ProductName")?.ToString();
            if (int.TryParse(build, out var buildNum))
            {
                isWin11 = buildNum >= 22000;
                winVersion = isWin11 ? $"Windows 11 (build {build})" : $"Windows 10 (build {build})";
            }
            else if (product != null) winVersion = product;
        }
        catch { }

        // DirectStorage: Windows 11 + NVMe SSD gerektirir; runtime registry yok,
        // OS sürümünü kontrol edip "destek olabilir" diye işaretliyoruz.
        bool dsSupported = isWin11;

        return new SystemState(hagsEnabled, hagsSupported, dsSupported, isWin11, winVersion);
    }
}
