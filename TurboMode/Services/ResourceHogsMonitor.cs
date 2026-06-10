using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using TurboMode.Config;

namespace TurboMode.Services;

/// <summary>
/// En çok RAM kullanan top N process'i canlı izler.
/// CPU% kalibrasyonu için iki örnek arası farkı kullanır.
/// </summary>
public sealed class ResourceHogsMonitor : IDisposable
{
    public sealed class HogItem
    {
        public string Name { get; init; } = "";
        public int Pid { get; init; }
        public int RamMb { get; set; }
        public bool IsWhitelisted { get; set; }
        public string DisplayRam => $"{RamMb} MB";
        public string Badge => IsWhitelisted ? "🛡 Korunan" : "🎯 Hedef";
    }

    public ObservableCollection<HogItem> Items { get; } = new();
    private DispatcherTimer? _timer;

    public void Start()
    {
        Probe();
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        { Interval = TimeSpan.FromSeconds(3) };
        _timer.Tick += (_, _) => Probe();
        _timer.Start();
    }

    public void Stop() { _timer?.Stop(); _timer = null; }

    private void Probe()
    {
        try
        {
            var whitelist = new HashSet<string>(OptimizationTargets.ProcessWhitelist,
                StringComparer.OrdinalIgnoreCase);

            var top = Process.GetProcesses()
                .Select(p =>
                {
                    try { return (Name: p.ProcessName, Id: p.Id, Mb: p.WorkingSet64 / 1024 / 1024); }
                    catch { return (Name: "", Id: 0, Mb: 0L); }
                    finally { p.Dispose(); }
                })
                .Where(t => t.Mb > 50 && !string.IsNullOrEmpty(t.Name))
                .OrderByDescending(t => t.Mb)
                .Take(10)
                .ToList();

            // UI thread'inde güncelle
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                foreach (var t in top)
                {
                    Items.Add(new HogItem
                    {
                        Name = t.Name,
                        Pid = t.Id,
                        RamMb = (int)t.Mb,
                        IsWhitelisted = whitelist.Contains(t.Name),
                    });
                }
            });
        }
        catch (Exception ex) { Log.Error(ex, "ResourceHogs probe hatası"); }
    }

    public void Dispose() => Stop();
}
