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
        }
    }

    private static bool Confirm(string text, string title)
    {
        var r = System.Windows.MessageBox.Show(text, title,
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return r == MessageBoxResult.Yes;
    }

    private static void Info(string text, string title) =>
        System.Windows.MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);

    private static void Error(string text) =>
        System.Windows.MessageBox.Show(text, "Fox Turbo Mod", MessageBoxButton.OK, MessageBoxImage.Error);

    private static void AskReboot(string message)
    {
        var r = System.Windows.MessageBox.Show(
            message + "\n\nŞimdi yeniden başlatmak ister misin?",
            "Yeniden Başlatma Gerek",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r == MessageBoxResult.Yes)
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
