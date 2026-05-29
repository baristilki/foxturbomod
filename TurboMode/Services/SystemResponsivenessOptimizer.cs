using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// Windows multimedia thread priority sistemini (MMCSS) gaming için optimize eder.
/// Görsel etki yok, CPU ödüllendirmesi oyun thread'ine kaydırılır.
/// %2-5 FPS — özellikle CPU bottleneck'lu oyunlarda (Valorant, CS2, Tarkov).
/// </summary>
public sealed class SystemResponsivenessOptimizer
{
    private const string MMCSSPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
    private const string GamesTaskPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games";

    private object? _prevSystemResp;
    private object? _prevGpuPrio;
    private object? _prevPrio;
    private object? _prevSchedCat;
    private object? _prevSfioPrio;

    public bool Activate()
    {
        try
        {
            using (var k = Registry.LocalMachine.CreateSubKey(MMCSSPath, true))
            {
                if (k == null) return false;
                _prevSystemResp = k.GetValue("SystemResponsiveness");
                // SystemResponsiveness: 0 = max gaming priority (default 20 = %20 multimedia rezerv)
                // NOT: NetworkThrottlingIndex'e DOKUNMUYORUZ — bazı online oyunlarda (CoD) latency artırabilir.
                k.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);
            }

            using (var k = Registry.LocalMachine.CreateSubKey(GamesTaskPath, true))
            {
                if (k == null) return false;
                _prevGpuPrio = k.GetValue("GPU Priority");
                _prevPrio = k.GetValue("Priority");
                _prevSchedCat = k.GetValue("Scheduling Category");
                _prevSfioPrio = k.GetValue("SFIO Priority");

                k.SetValue("GPU Priority", 8, RegistryValueKind.DWord);  // 0-31, default 5
                k.SetValue("Priority", 6, RegistryValueKind.DWord);       // CPU prio, default 2
                k.SetValue("Scheduling Category", "High", RegistryValueKind.String);
                k.SetValue("SFIO Priority", "High", RegistryValueKind.String);
            }
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        try
        {
            using (var k = Registry.LocalMachine.CreateSubKey(MMCSSPath, true))
            {
                if (k != null)
                {
                    RestoreOrDelete(k, "SystemResponsiveness", _prevSystemResp);
                }
            }
            using (var k = Registry.LocalMachine.CreateSubKey(GamesTaskPath, true))
            {
                if (k != null)
                {
                    RestoreOrDelete(k, "GPU Priority", _prevGpuPrio);
                    RestoreOrDelete(k, "Priority", _prevPrio);
                    RestoreOrDelete(k, "Scheduling Category", _prevSchedCat);
                    RestoreOrDelete(k, "SFIO Priority", _prevSfioPrio);
                }
            }
        }
        catch { }
    }

    private static void RestoreOrDelete(RegistryKey k, string name, object? prev)
    {
        if (prev == null) { try { k.DeleteValue(name, false); } catch { } }
        else k.SetValue(name, prev);
    }
}
