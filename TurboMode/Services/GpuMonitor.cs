using System.Diagnostics;
using System.Globalization;

namespace TurboMode.Services;

/// <summary>
/// GPU sıcaklık ve kullanım okur. NVIDIA için nvidia-smi, AMD/Intel için WMI fallback.
/// </summary>
public sealed class GpuMonitor : IDisposable
{
    public event Action<GpuInfo>? Updated;

    public sealed record GpuInfo(
        string Name,
        int TempC,
        int UsagePercent,
        int MemoryUsedMb,
        int MemoryTotalMb,
        string Source);

    private System.Windows.Threading.DispatcherTimer? _timer;
    private bool _nvidiaSmiAvailable = true;

    public void Start()
    {
        _timer = new System.Windows.Threading.DispatcherTimer(
            System.Windows.Threading.DispatcherPriority.Background)
        { Interval = TimeSpan.FromSeconds(3) };
        _timer.Tick += (_, _) => Probe();
        _timer.Start();
        Probe();
    }

    public void Stop() { _timer?.Stop(); _timer = null; }

    private void Probe()
    {
        Task.Run(() =>
        {
            try
            {
                if (_nvidiaSmiAvailable)
                {
                    var info = ProbeNvidia();
                    if (info != null) { Updated?.Invoke(info); return; }
                }
                var fallback = ProbeWmi();
                if (fallback != null) Updated?.Invoke(fallback);
            }
            catch (Exception ex) { Log.Error(ex, "GPU probe hatası"); }
        });
    }

    private GpuInfo? ProbeNvidia()
    {
        try
        {
            var psi = new ProcessStartInfo("nvidia-smi.exe",
                "--query-gpu=name,temperature.gpu,utilization.gpu,memory.used,memory.total " +
                "--format=csv,noheader,nounits")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            if (p == null) return null;
            var line = p.StandardOutput.ReadLine();
            p.WaitForExit(2000);

            if (string.IsNullOrEmpty(line))
            {
                if (p.ExitCode != 0) _nvidiaSmiAvailable = false;
                return null;
            }

            var parts = line.Split(',').Select(s => s.Trim()).ToArray();
            if (parts.Length < 5) return null;

            int.TryParse(parts[1], out var temp);
            int.TryParse(parts[2], out var util);
            int.TryParse(parts[3], out var memUsed);
            int.TryParse(parts[4], out var memTotal);

            return new GpuInfo(parts[0], temp, util, memUsed, memTotal, "NVIDIA");
        }
        catch
        {
            _nvidiaSmiAvailable = false;
            return null;
        }
    }

    private GpuInfo? ProbeWmi()
    {
        try
        {
            using var s = new System.Management.ManagementObjectSearcher(
                "SELECT Name, AdapterRAM FROM Win32_VideoController");
            foreach (var o in s.Get())
            {
                var name = o["Name"]?.ToString() ?? "GPU";
                var ram = 0;
                try { ram = (int)(Convert.ToInt64(o["AdapterRAM"]) / 1024 / 1024); } catch { }
                return new GpuInfo(name, 0, 0, 0, ram, "WMI");
            }
        }
        catch { }
        return null;
    }

    public void Dispose() => Stop();
}
