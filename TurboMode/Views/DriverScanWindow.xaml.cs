using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using TurboMode.Services;
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;

namespace TurboMode.Views;

public partial class DriverScanWindow : Window
{
    public DriverScanWindow()
    {
        InitializeComponent();
        Rescan();
    }

    private async void Rescan()
    {
        try
        {
            StatusLabel.Text = "Taranıyor...";
            Log.Info("DriverScan: tarama başlıyor");
            var drivers = await Task.Run(() => DriverChecker.Scan());
            Log.Info("DriverScan: {0} sürücü bulundu", drivers.Count);
            var items = drivers.Select(d => new DriverVm(d)).ToList();
            DriversList.ItemsSource = items;
            int eski = items.Count(i => i.Health.StartsWith("Eski"));
            StatusLabel.Text = $"{drivers.Count} sürücü bulundu" + (eski > 0 ? $" — {eski} tanesi eski" : " — hepsi güncel");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Driver scan hatası");
            StatusLabel.Text = "Tarama hatası: " + ex.Message;
            FoxDialog.Error(this, "Sürücü Taraması", "Tarama hatası:\n\n" + ex.Message);
        }
    }

    private void Rescan_Click(object sender, RoutedEventArgs e) => Rescan();

    private void OpenDriverUrl_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string url && !string.IsNullOrEmpty(url))
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }
    }

    public class DriverVm
    {
        public string Category { get; }
        public string Name { get; }
        public string Vendor { get; }
        public string Version { get; }
        public DateTime? Date { get; }
        public string Health { get; }
        public string? DownloadUrl { get; }
        public bool HasUrl => !string.IsNullOrEmpty(DownloadUrl);
        public Brush HealthBrush { get; }

        public DriverVm(DriverChecker.DriverInfo d)
        {
            Category = d.Category;
            Name = d.Name;
            Vendor = d.Vendor;
            Version = d.Version;
            Date = d.Date;
            Health = d.Health;
            DownloadUrl = d.DownloadUrl;
            HealthBrush = d.Health switch
            {
                var h when h.StartsWith("Eski (2") => new SolidColorBrush(Color.FromRgb(0xE0, 0x40, 0x40)),
                var h when h.StartsWith("Eski") => new SolidColorBrush(Color.FromRgb(0xCC, 0x99, 0x33)),
                _ => new SolidColorBrush(Color.FromRgb(0x3A, 0x88, 0x4A)),
            };
        }
    }
}
