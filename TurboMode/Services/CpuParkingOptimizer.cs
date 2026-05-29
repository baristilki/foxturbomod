using System.Diagnostics;

namespace TurboMode.Services;

/// <summary>
/// CPU core parking'i kapatır (tüm çekirdekler her zaman aktif).
/// High Performance plan zaten çoğu sistemde bunu yapar; bu sadece edge case'leri yakalar.
/// </summary>
public sealed class CpuParkingOptimizer
{
    private bool _wasActive;

    public bool Activate()
    {
        try
        {
            // CPMINCORES = 100 → minimum çekirdek sayısı %100 (hiç parking yok)
            Run("/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100");
            Run("/setdcvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100");
            Run("/setactive SCHEME_CURRENT");
            _wasActive = true;
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        if (!_wasActive) return;
        try
        {
            // Varsayılan değer 5 (Windows default), bazen 10. 5'e döndür güvenli.
            Run("/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 5");
            Run("/setdcvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 5");
            Run("/setactive SCHEME_CURRENT");
        }
        catch { }
        _wasActive = false;
    }

    private static void Run(string args)
    {
        var psi = new ProcessStartInfo("powercfg.exe", args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        };
        using var p = Process.Start(psi);
        p?.WaitForExit(3000);
    }
}
