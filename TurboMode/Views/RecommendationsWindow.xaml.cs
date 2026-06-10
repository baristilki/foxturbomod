using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using TurboMode.Models;
using TurboMode.Services;
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;

namespace TurboMode.Views;

public partial class RecommendationsWindow : Window
{
    // GoodbyeDPI instance — uygulama yaşam süresi boyunca yaşar.
    // Pencere kapansa bile DPI bypass arka planda çalışmaya devam eder.
    private static readonly DpiBypass _dpi = new();

    public RecommendationsWindow()
    {
        InitializeComponent();
        Load();
    }

    private void Load()
    {
        var state = SystemStateChecker.Check();
        var items = RecommendationsService.Build(state);
        RecList.ItemsSource = items.Select(r => new RecVm(r)).ToList();
    }

    private async Task ProbeBackground()
    {
        // Network ping ölçümü
        try
        {
            var pings = await NetworkProbe.ProbeAsync();
            var summary = string.Join("  •  ",
                pings.Select(p => p.Success ? $"{p.Target}: {p.PingMs}ms" : $"{p.Target}: ✗"));
            RecommendationsServiceHelpers.LastNetworkSummary = summary;
        }
        catch (Exception ex) { Log.Error(ex, "Network probe hatası"); }

        // Driver bilgileri
        try
        {
            var drivers = await Task.Run(() => DriverChecker.Check());
            if (drivers.Count > 0)
            {
                var sum = string.Join("  •  ",
                    drivers.Select(d => $"{d.Vendor} {d.Version}"));
                RecommendationsServiceHelpers.LastDriverSummary = sum;
            }
            else RecommendationsServiceHelpers.LastDriverSummary = "Sürücü bulunamadı";
        }
        catch (Exception ex) { Log.Error(ex, "Driver check hatası"); }

        // Listeyi tazele
        Dispatcher.Invoke(Load);
    }

    private void Action_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.Tag is not Recommendation r) return;

        if (!string.IsNullOrEmpty(r.ActionCommand))
        {
            HandleCommand(r);
            return;
        }

        if (!string.IsNullOrEmpty(r.ActionUrl))
        {
            try { Process.Start(new ProcessStartInfo(r.ActionUrl) { UseShellExecute = true }); }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Link açılamadı: {ex.Message}",
                    "Fox Turbo Mod", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void HandleCommand(Recommendation r)
    {
        switch (r.ActionCommand)
        {
            case "clear-shader-cache":
                {
                    var result = ShaderCacheCleaner.Clean();
                    var mb = Math.Round(result.FreedBytes / 1024.0 / 1024.0, 1);
                    Info($"✓ {result.DeletedFiles} dosya silindi\n✓ {mb} MB serbest", "Shader Cache");
                    break;
                }

            case "disable-vbs":
                {
                    if (!Confirm(
                        "VBS / Bellek Bütünlüğü kapatılacak.\n\n" +
                        "• Etkili olması için bilgisayarı yeniden başlatman gerek.\n" +
                        "• Güvenlik özelliği — kötü amaçlı driver koruması düşer.\n" +
                        "• %5-15 FPS kazancı (özellikle Ryzen).\n\n" +
                        "Devam etmek istiyor musun?", "VBS Kapat")) return;
                    var rr = VbsToggler.Disable();
                    if (rr.Success) AskReboot(rr.Message); else Error(rr.Message);
                    Load();
                    break;
                }

            case "disable-hypervisor":
                {
                    if (!Confirm(
                        "Hipervizor başlangıçta yüklenmeyecek.\n\n" +
                        "• WSL2, Docker, Hyper-V VM, Windows Sandbox kullanılamaz olacak.\n" +
                        "• Reboot gerek.\n" +
                        "• %3-8 FPS kazancı.\n\n" +
                        "Devam?", "Hipervizor Kapat")) return;
                    var rr = HypervisorToggler.Disable();
                    if (rr.Success) AskReboot(rr.Message); else Error(rr.Message);
                    Load();
                    break;
                }

            case "close-discord":
                {
                    if (!Confirm(
                        "Discord tamamen kapatılacak (overlay'i durdurmak için).\n" +
                        "Sesli görüşmedeysen kesilir. Devam?", "Discord Kapat")) return;
                    var rr = OverlayDisabler.CloseDiscord();
                    Info(rr.Message, "Discord");
                    Load();
                    break;
                }

            case "close-nvidia-overlay":
                {
                    var rr = OverlayDisabler.CloseNvidiaOverlay();
                    Info(rr.Message, "NVIDIA Overlay");
                    Load();
                    break;
                }

            case "disable-background-apps":
                {
                    var rr = WindowsTweaks.DisableBackgroundApps();
                    if (rr.Success) Info(rr.Message, "Arka Plan Uygulamaları");
                    else Error(rr.Message);
                    break;
                }

            case "disable-telemetry-tasks":
                {
                    var rr = WindowsTweaks.DisableTelemetryTasks();
                    Info(rr.Message, "Telemetri Görevleri");
                    break;
                }

            case "open-nvcpl":
                {
                    var rr = WindowsTweaks.OpenNvidiaControlPanel();
                    if (!rr.Success) Error(rr.Message);
                    break;
                }

            case "dns-and-discord":
                {
                    if (!Confirm(
                        "Aktif ağ adaptörlerinin DNS sunucusunu Cloudflare (1.1.1.1) yapacağım, " +
                        "DNS cache'i temizleyeceğim ve Discord'u başlatacağım.\n\n" +
                        "İstediğin zaman 'DNS'i Geri Al' diyebilirsin. Devam?", "DNS + Discord")) return;
                    var rr = DnsOptimizer.OpenDiscordWithDns();
                    if (rr.Success) Info(rr.Message, "DNS Değiştirildi");
                    else Error(rr.Message);
                    break;
                }

            case "dns-restore":
                {
                    var rr = DnsOptimizer.RestoreAutomatic();
                    if (rr.Success) Info(rr.Message, "DNS Geri Yüklendi");
                    else Error(rr.Message);
                    break;
                }

            case "dpi-and-discord":
                {
                    if (!Confirm(
                        "GoodbyeDPI başlatılacak (TLS paketlerini parçalayarak DPI engellemesini atlatır) " +
                        "ve Discord açılacak.\n\n" +
                        "• Kernel driver yüklenir (WinDivert) — yasal, açık kaynak (MIT).\n" +
                        "• Valorant/Vanguard açmadan önce mutlaka 'Durdur' butonuna bas — driver çakışması olabilir.\n" +
                        "• Tüm internet trafiğin parçalandığı için %1-2 latency artışı olabilir.\n\n" +
                        "Devam?", "DPI Bypass + Discord")) return;
                    var rr = _dpi.OpenDiscordWithBypass();
                    if (rr.Success) Info(rr.Message, "DPI Bypass Aktif");
                    else Error(rr.Message);
                    break;
                }

            case "clean-temp":
                {
                    var rr = WindowsTweaks.CleanTempFolders();
                    if (rr.Success) Info(rr.Message, "TEMP Temizlendi"); else Error(rr.Message);
                    break;
                }

            case "dpi-stop":
                {
                    var rr = _dpi.Stop();
                    Info(rr.Message, "GoodbyeDPI");
                    break;
                }

            case "open-driver-scan":
                {
                    try
                    {
                        var win = new DriverScanWindow { Owner = this };
                        win.ShowDialog();
                    }
                    catch (Exception ex) { Error(ex.Message); }
                    break;
                }
        }
    }

    private bool Confirm(string text, string title) => FoxDialog.Confirm(this, title, text);
    private void Info(string text, string title) => FoxDialog.Info(this, title, text);
    private void Error(string text) => FoxDialog.Error(this, "Fox Turbo Mod", text);

    private void AskReboot(string message)
    {
        var r = FoxDialog.Show(this, "Yeniden Başlatma Gerek",
            message + "\n\nŞimdi yeniden başlatmak ister misin?",
            FoxDialog.DialogKind.Confirm, "Sonra", "Şimdi Başlat");
        if (r == "Şimdi Başlat")
        {
            try
            {
                Process.Start(new ProcessStartInfo("shutdown.exe", "/r /t 10 /c \"Fox Turbo Mod: 10 sn sonra reboot\"")
                { UseShellExecute = false, CreateNoWindow = true });
            }
            catch { }
        }
    }

    public class RecVm
    {
        public Recommendation Item { get; }
        public Brush BackgroundBrush { get; }
        public Brush BorderBrush { get; }
        public Brush ImpactBrush { get; }
        public bool HasImpact => !string.IsNullOrEmpty(Item.FpsImpact);
        public bool HasAction => !string.IsNullOrEmpty(Item.ActionLabel);

        public RecVm(Recommendation r)
        {
            Item = r;
            (BackgroundBrush, BorderBrush, ImpactBrush) = r.Severity switch
            {
                RecommendationSeverity.Critical => (
                    new SolidColorBrush(Color.FromRgb(0x2A, 0x10, 0x10)),
                    new SolidColorBrush(Color.FromRgb(0xC0, 0x3A, 0x3A)),
                    new SolidColorBrush(Color.FromRgb(0xC0, 0x3A, 0x3A))),
                RecommendationSeverity.Warning => (
                    new SolidColorBrush(Color.FromRgb(0x2A, 0x20, 0x10)),
                    new SolidColorBrush(Color.FromRgb(0xCC, 0x99, 0x33)),
                    new SolidColorBrush(Color.FromRgb(0xCC, 0x99, 0x33))),
                RecommendationSeverity.Good => (
                    new SolidColorBrush(Color.FromRgb(0x10, 0x20, 0x14)),
                    new SolidColorBrush(Color.FromRgb(0x3A, 0x88, 0x4A)),
                    new SolidColorBrush(Color.FromRgb(0x3A, 0x88, 0x4A))),
                _ => (
                    new SolidColorBrush(Color.FromRgb(0x18, 0x18, 0x20)),
                    new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x3A)),
                    new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x60))),
            };
        }
    }
}
