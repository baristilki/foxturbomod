using System.Diagnostics;

namespace TurboMode.Services;

/// <summary>
/// Hypervisor Launch Type'ı off yapar. WSL2, Docker, Hyper-V, Windows Sandbox etkilenir.
/// Reboot gerektirir.
/// </summary>
public static class HypervisorToggler
{
    public sealed record Result(bool Success, string Message, bool RebootRequired);

    public static Result Disable()
    {
        try
        {
            Run("/set hypervisorlaunchtype off");
            return new Result(true,
                "Hypervisor başlangıçta yüklenmeyecek (WSL2/Docker/Sandbox kullanamazsın). " +
                "Bilgisayarı YENİDEN BAŞLATMAN gerek.",
                true);
        }
        catch (Exception ex) { return new Result(false, ex.Message, false); }
    }

    public static Result Enable()
    {
        try
        {
            Run("/set hypervisorlaunchtype auto");
            return new Result(true, "Hypervisor tekrar aktif. Reboot gerek.", true);
        }
        catch (Exception ex) { return new Result(false, ex.Message, false); }
    }

    private static void Run(string args)
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
}
