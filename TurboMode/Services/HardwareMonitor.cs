using LibreHardwareMonitor.Hardware;

namespace TurboMode.Services;

/// <summary>
/// CPU + GPU sıcaklık ve yük okuma. LibreHardwareMonitorLib kullanır (admin gerek — bizde var).
/// Her 2 sn'de günceller.
/// </summary>
public sealed class HardwareMonitor : IDisposable
{
    public event Action<HardwareSnapshot>? Updated;

    public sealed record HardwareSnapshot(
        string CpuName, float CpuTempC, float CpuLoadPercent,
        string GpuName, float GpuTempC, float GpuLoadPercent,
        float RamUsedGb, float RamTotalGb);

    private Computer? _computer;
    private System.Windows.Threading.DispatcherTimer? _timer;

    private sealed class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer c) => c.Traverse(this);
        public void VisitHardware(IHardware h) { h.Update(); foreach (var s in h.SubHardware) s.Update(); }
        public void VisitSensor(ISensor s) { }
        public void VisitParameter(IParameter p) { }
    }

    public void Start()
    {
        try
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = false,
                IsStorageEnabled = false,
                IsNetworkEnabled = false,
            };
            _computer.Open();
        }
        catch (Exception ex) { Log.Error(ex, "HardwareMonitor başlatılamadı"); return; }

        _timer = new System.Windows.Threading.DispatcherTimer(
            System.Windows.Threading.DispatcherPriority.Background)
        { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += (_, _) => Probe();
        _timer.Start();
        Probe();
    }

    private void Probe()
    {
        if (_computer == null) return;
        Task.Run(() =>
        {
            try
            {
                _computer.Accept(new UpdateVisitor());

                string cpuName = "CPU";
                float cpuTemp = 0, cpuLoad = 0;
                string gpuName = "GPU";
                float gpuTemp = 0, gpuLoad = 0;
                float ramUsed = 0, ramTotal = 0;

                foreach (var hw in _computer.Hardware)
                {
                    if (hw.HardwareType == HardwareType.Cpu)
                    {
                        cpuName = hw.Name;
                        foreach (var s in hw.Sensors)
                        {
                            if (s.SensorType == SensorType.Temperature && s.Name.Contains("Package", StringComparison.OrdinalIgnoreCase))
                                cpuTemp = s.Value ?? 0;
                            else if (s.SensorType == SensorType.Temperature && cpuTemp == 0 && s.Name.Contains("Tctl", StringComparison.OrdinalIgnoreCase))
                                cpuTemp = s.Value ?? 0;
                            else if (s.SensorType == SensorType.Temperature && cpuTemp == 0)
                                cpuTemp = Math.Max(cpuTemp, s.Value ?? 0);
                            if (s.SensorType == SensorType.Load && s.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
                                cpuLoad = s.Value ?? 0;
                        }
                    }
                    else if (hw.HardwareType == HardwareType.GpuNvidia ||
                             hw.HardwareType == HardwareType.GpuAmd ||
                             hw.HardwareType == HardwareType.GpuIntel)
                    {
                        gpuName = hw.Name;
                        foreach (var s in hw.Sensors)
                        {
                            if (s.SensorType == SensorType.Temperature && s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                                gpuTemp = s.Value ?? 0;
                            else if (s.SensorType == SensorType.Temperature && gpuTemp == 0)
                                gpuTemp = s.Value ?? 0;
                            if (s.SensorType == SensorType.Load && s.Name.Contains("Core", StringComparison.OrdinalIgnoreCase))
                                gpuLoad = s.Value ?? 0;
                            else if (s.SensorType == SensorType.Load && gpuLoad == 0)
                                gpuLoad = s.Value ?? 0;
                        }
                    }
                    else if (hw.HardwareType == HardwareType.Memory)
                    {
                        foreach (var s in hw.Sensors)
                        {
                            if (s.SensorType == SensorType.Data && s.Name.Contains("Used", StringComparison.OrdinalIgnoreCase))
                                ramUsed = s.Value ?? 0;
                            else if (s.SensorType == SensorType.Data && s.Name.Contains("Available", StringComparison.OrdinalIgnoreCase))
                                ramTotal += (s.Value ?? 0);
                        }
                    }
                }
                if (ramTotal > 0) ramTotal += ramUsed;

                Updated?.Invoke(new HardwareSnapshot(
                    cpuName, cpuTemp, cpuLoad,
                    gpuName, gpuTemp, gpuLoad,
                    ramUsed, ramTotal));
            }
            catch (Exception ex) { Log.Error(ex, "HardwareMonitor probe hatası"); }
        });
    }

    public void Dispose()
    {
        try { _timer?.Stop(); _computer?.Close(); }
        catch { }
        _computer = null;
    }
}
