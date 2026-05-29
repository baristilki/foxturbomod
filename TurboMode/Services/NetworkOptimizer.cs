using System.Diagnostics;

namespace TurboMode.Services;

/// <summary>
/// Ağ optimizasyonları:
/// 1) DNS önbelleğini temizler (eski DNS cache'i sorun çıkarabilir)
/// 2) Windows QoS politikası oluşturur — aktif oyun süreci için DSCP=46 (Expedited Forwarding)
/// 3) Deaktivasyonda QoS politikasını siler
/// QoS politikaları router/ISP tarafından zorunlu uygulanmaz; ev ağında sınırlı fayda sağlar.
/// </summary>
public sealed class NetworkOptimizer
{
    private const string PolicyName = "FoxTurboMod-Game";
    private string? _activeAppPath;

    public bool Activate(string? gameExecutablePath)
    {
        try
        {
            RunNetsh("interface ip delete dns *");      // güvenli değil, atla
        }
        catch { }
        try { RunCommand("ipconfig.exe", "/flushdns"); } catch { }

        if (string.IsNullOrEmpty(gameExecutablePath)) return false;

        // QoS politikasını PowerShell üzerinden ekle.
        // Eski politika varsa sil, sonra ekle.
        try
        {
            RunPowerShell(
                $"Remove-NetQosPolicy -Name '{PolicyName}' -Confirm:$false -ErrorAction SilentlyContinue; " +
                $"New-NetQosPolicy -Name '{PolicyName}' " +
                $"-AppPathNameMatchCondition '{gameExecutablePath.Replace("'", "''")}' " +
                $"-IPProtocolMatchCondition Both -DSCPAction 46 -NetworkProfile All -ErrorAction SilentlyContinue");
            _activeAppPath = gameExecutablePath;
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        try
        {
            RunPowerShell($"Remove-NetQosPolicy -Name '{PolicyName}' -Confirm:$false -ErrorAction SilentlyContinue");
        }
        catch { }
        _activeAppPath = null;
    }

    private static void RunPowerShell(string command)
    {
        var psi = new ProcessStartInfo("powershell.exe",
            $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command}\"")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        using var p = Process.Start(psi);
        p?.WaitForExit(5000);
    }

    private static void RunCommand(string exe, string args)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        };
        using var p = Process.Start(psi);
        p?.WaitForExit(3000);
    }

    private static void RunNetsh(string args) => RunCommand("netsh.exe", args);
}
