using System.Diagnostics;
using System.Management;
using System.Windows.Threading;

namespace TurboMode.Services;

/// <summary>
/// CPU ve RAM kullanımını 1 sn aralıkla okur, UI'a yayınlar.
/// </summary>
public sealed class SystemMetrics
{
    public event Action<double, double, double>? Updated; // cpu%, ramUsedGb, ramTotalGb

    private readonly PerformanceCounter _cpuCounter =
        new("Processor", "% Processor Time", "_Total");
    private readonly double _totalRamGb;
    private DispatcherTimer? _timer;

    public SystemMetrics()
    {
        _totalRamGb = GetTotalRamGb();
        _ = _cpuCounter.NextValue(); // ilk okuma her zaman 0 döner, atıyoruz
    }

    public void Start()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (_, _) =>
        {
            double cpu = Math.Clamp(_cpuCounter.NextValue(), 0, 100);
            double freeRamGb = GetFreeRamGb();
            double usedGb = Math.Max(0, _totalRamGb - freeRamGb);
            Updated?.Invoke(cpu, usedGb, _totalRamGb);
        };
        _timer.Start();
    }

    private static double GetTotalRamGb()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                var kb = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                return kb / 1024.0 / 1024.0;
            }
        }
        catch { }
        return 0;
    }

    private static double GetFreeRamGb()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                var kb = Convert.ToDouble(obj["FreePhysicalMemory"]);
                return kb / 1024.0 / 1024.0;
            }
        }
        catch { }
        return 0;
    }
}
