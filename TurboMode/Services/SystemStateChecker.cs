using System.Diagnostics;
using Microsoft.Win32;

namespace TurboMode.Services;

public static class SystemStateChecker
{
    public record SystemState(
        bool HagsEnabled,
        bool HagsSupported,
        bool DirectStorageSupported,
        bool IsWindows11,
        string WindowsVersion,
        bool VbsEnabled,
        bool HvciEnabled,
        bool HypervisorPresent,
        bool DiscordRunning,
        bool GeforceOverlayRunning);

    public static SystemState Check()
    {
        bool hagsEnabled = false, hagsSupported = false;
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers");
            var v = k?.GetValue("HwSchMode");
            if (v is int mode) { hagsSupported = true; hagsEnabled = mode == 2; }
        }
        catch { }

        bool isWin11 = false;
        string winVersion = "Unknown";
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var build = k?.GetValue("CurrentBuild")?.ToString();
            if (int.TryParse(build, out var buildNum))
            {
                isWin11 = buildNum >= 22000;
                winVersion = isWin11 ? $"Windows 11 (build {build})" : $"Windows 10 (build {build})";
            }
        }
        catch { }

        // VBS — Virtualization Based Security
        bool vbsEnabled = false;
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard");
            var v = k?.GetValue("EnableVirtualizationBasedSecurity");
            if (v is int n) vbsEnabled = n == 1;
        }
        catch { }

        // HVCI / Memory Integrity
        bool hvciEnabled = false;
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity");
            var v = k?.GetValue("Enabled");
            if (v is int n) hvciEnabled = n == 1;
        }
        catch { }

        // Hyper-V / hypervisor varlığı (WSL2, Docker, Sandbox veya tam Hyper-V)
        bool hypervisorPresent = false;
        try
        {
            using var s = new System.Management.ManagementObjectSearcher(
                "SELECT HypervisorPresent FROM Win32_ComputerSystem");
            foreach (var o in s.Get())
            {
                hypervisorPresent = Convert.ToBoolean(o["HypervisorPresent"]);
                break;
            }
        }
        catch { }

        // Discord ve GeForce Experience overlay process kontrolü
        bool discord = Process.GetProcessesByName("Discord").Any() ||
                       Process.GetProcessesByName("DiscordCanary").Any() ||
                       Process.GetProcessesByName("DiscordPTB").Any();
        bool gfe = Process.GetProcessesByName("NVIDIA Overlay").Any() ||
                   Process.GetProcessesByName("nvcontainer").Any();

        bool dsSupported = isWin11;

        return new SystemState(
            hagsEnabled, hagsSupported, dsSupported, isWin11, winVersion,
            vbsEnabled, hvciEnabled, hypervisorPresent, discord, gfe);
    }
}
