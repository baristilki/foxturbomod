using System.Diagnostics;

namespace TurboMode.Services;

/// <summary>
/// Windows güç planını "Yüksek Performans"a geçirir. Laptop'larda büyük fark,
/// masaüstünde küçük ama hissedilir (CPU/PCIe sleep state agresifliği azalır).
/// Deactivate eski planı geri yükler.
/// </summary>
public sealed class PowerPlanOptimizer
{
    // Standart Windows GUID'leri
    private const string GuidHighPerformance = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";

    private string? _previousGuid;

    public bool Activate()
    {
        try
        {
            _previousGuid = RunPowercfg("/getactivescheme");
            // Çıktı örneği: "Güç Düzeni GUID'i: 381b4222-... (Dengeli)"
            if (_previousGuid != null)
            {
                var m = System.Text.RegularExpressions.Regex.Match(
                    _previousGuid, @"([0-9a-fA-F\-]{36})");
                _previousGuid = m.Success ? m.Groups[1].Value : null;
            }
            RunPowercfg($"/setactive {GuidHighPerformance}");
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        if (string.IsNullOrEmpty(_previousGuid)) return;
        try { RunPowercfg($"/setactive {_previousGuid}"); } catch { }
        _previousGuid = null;
    }

    private static string? RunPowercfg(string args)
    {
        var psi = new ProcessStartInfo("powercfg.exe", args)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
        };
        using var p = Process.Start(psi);
        if (p == null) return null;
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit(3000);
        return output;
    }
}
