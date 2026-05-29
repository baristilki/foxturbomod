using System.Diagnostics;
using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// VBS (Virtualization Based Security) ve HVCI (Memory Integrity) kapatır.
/// Reboot gerektirir. Registry değişikliği + bcdedit hypervisorlaunchtype off.
/// </summary>
public static class VbsToggler
{
    public sealed record Result(bool Success, string Message, bool RebootRequired);

    public static Result Disable()
    {
        try
        {
            // 1. DeviceGuard ana ayarları
            using (var k = Registry.LocalMachine.CreateSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard", true))
            {
                if (k == null) return new Result(false, "DeviceGuard kayıt anahtarı açılamadı", false);
                k.SetValue("EnableVirtualizationBasedSecurity", 0, RegistryValueKind.DWord);
                k.SetValue("RequirePlatformSecurityFeatures", 0, RegistryValueKind.DWord);
                k.SetValue("HypervisorEnforcedCodeIntegrity", 0, RegistryValueKind.DWord);
                k.SetValue("Locked", 0, RegistryValueKind.DWord);
            }

            // 2. HVCI / Memory Integrity
            using (var k = Registry.LocalMachine.CreateSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", true))
            {
                k?.SetValue("Enabled", 0, RegistryValueKind.DWord);
                k?.SetValue("Locked", 0, RegistryValueKind.DWord);
            }

            // 3. Credential Guard
            using (var k = Registry.LocalMachine.CreateSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\CredentialGuard", true))
            {
                k?.SetValue("Enabled", 0, RegistryValueKind.DWord);
                k?.SetValue("Locked", 0, RegistryValueKind.DWord);
            }

            // 4. LSA — Local Security Authority Credential Guard
            using (var k = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Lsa", true))
            {
                k?.SetValue("LsaCfgFlags", 0, RegistryValueKind.DWord);
            }

            // 5. Hypervisor başlangıç tipi (BCD boot konfigürasyonu)
            // VBS kapatılsa bile hypervisor başlatılırsa CPU overhead devam eder
            RunBcdedit("/set hypervisorlaunchtype off");

            return new Result(true,
                "VBS / Bellek Bütünlüğü ayarları kapatıldı. " +
                "Değişikliklerin etkili olması için bilgisayarı YENİDEN BAŞLATMAN gerek.",
                RebootRequired: true);
        }
        catch (Exception ex)
        {
            return new Result(false, $"Hata: {ex.Message}", false);
        }
    }

    /// <summary>Geri aç (varsayılan Windows ayarına döndür).</summary>
    public static Result Enable()
    {
        try
        {
            using (var k = Registry.LocalMachine.CreateSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard", true))
            {
                k?.SetValue("EnableVirtualizationBasedSecurity", 1, RegistryValueKind.DWord);
                k?.SetValue("HypervisorEnforcedCodeIntegrity", 1, RegistryValueKind.DWord);
            }
            using (var k = Registry.LocalMachine.CreateSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", true))
            {
                k?.SetValue("Enabled", 1, RegistryValueKind.DWord);
            }
            RunBcdedit("/set hypervisorlaunchtype auto");
            return new Result(true, "VBS / Bellek Bütünlüğü tekrar aktif. Reboot gerek.", true);
        }
        catch (Exception ex) { return new Result(false, ex.Message, false); }
    }

    private static void RunBcdedit(string args)
    {
        try
        {
            var psi = new ProcessStartInfo("bcdedit.exe", args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(5000);
        }
        catch { }
    }
}
