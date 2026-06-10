using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TurboMode.Models;
using TurboMode.Services;
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;

namespace TurboMode.Views;

public partial class OnboardingWindow : Window
{
    private int _page = 1;
    private readonly AppSettings _settings;
    private bool _actionsLoaded;

    public OnboardingWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
    }

    private void Show(int page)
    {
        _page = page;
        Page1.Visibility = page == 1 ? Visibility.Visible : Visibility.Collapsed;
        Page2.Visibility = page == 2 ? Visibility.Visible : Visibility.Collapsed;
        Page3.Visibility = page == 3 ? Visibility.Visible : Visibility.Collapsed;

        var accent = (Brush)FindResource("AccentBrush");
        var dim = (Brush)FindResource("BorderBrushDim");
        Dot1.Fill = page >= 1 ? accent : dim;
        Dot2.Fill = page >= 2 ? accent : dim;
        Dot3.Fill = page >= 3 ? accent : dim;

        PrevButton.IsEnabled = page > 1;
        NextButton.Content = page == 3 ? "Tamam ✓" : "Sonraki ▶";

        // Sayfa 3'e geçince dinamik öneri kartlarını yükle (sadece bir kez)
        if (page == 3 && !_actionsLoaded)
        {
            _actionsLoaded = true;
            _ = LoadActionsAsync();
        }
    }

    private async Task LoadActionsAsync()
    {
        // Sistemi analiz et: VBS, HAGS, Hipervizor, Discord ping, Driver yaşı
        var state = await Task.Run(() => SystemStateChecker.Check());
        var actions = new List<ActionItem>();

        // 1. VBS / HVCI — en yüksek FPS etkili
        if (state.VbsEnabled || state.HvciEnabled)
        {
            actions.Add(new ActionItem
            {
                Icon = "🛑", Priority = 1,
                Title = "VBS / Bellek Bütünlüğü AÇIK",
                Description = "Ryzen sistemde %5-15 FPS yer. Reboot gerek. Güvenlik özelliği — kararını sen ver.",
                Badge = "%5-15 FPS",
                BadgeColor = "#E04040",
                Action = "disable-vbs",
            });
        }

        // 2. Discord erişim — ping test
        try
        {
            var pings = await NetworkProbe.ProbeAsync();
            var discord = pings.FirstOrDefault(p => p.Target == "Discord");
            if (discord != null && !discord.Success)
            {
                actions.Add(new ActionItem
                {
                    Icon = "💬", Priority = 1,
                    Title = "Discord açılmıyor — DPI Bypass öner",
                    Description = "ISP Discord'a erişimi engelliyor. GoodbyeDPI ile bypass et + Discord'u başlat.",
                    Badge = "Erişim Yok",
                    BadgeColor = "#E04040",
                    Action = "dns-and-discord",
                });
            }
        }
        catch { }

        // 3. Hipervizor
        if (state.HypervisorPresent)
        {
            actions.Add(new ActionItem
            {
                Icon = "⚠", Priority = 2,
                Title = "Hipervizor aktif",
                Description = "Hyper-V / WSL2 / Docker / Sandbox. Kullanmıyorsan kapat — %3-8 FPS kazanç.",
                Badge = "%3-8 FPS",
                BadgeColor = "#CC9933",
                Action = "disable-hypervisor",
            });
        }

        // 4. HAGS kapalı
        if (state.HagsSupported && !state.HagsEnabled)
        {
            actions.Add(new ActionItem
            {
                Icon = "💡", Priority = 2,
                Title = "HAGS kapalı (GPU Scheduling)",
                Description = "Hardware-Accelerated GPU Scheduling açık olmalı. Manuel açman gerek (Windows ayarları).",
                Badge = "%2-5 FPS",
                BadgeColor = "#CC9933",
                Action = "open-graphics-settings",
            });
        }

        // 5. Discord overlay (çalışıyorsa kapat öner)
        if (state.DiscordRunning)
        {
            actions.Add(new ActionItem
            {
                Icon = "💬", Priority = 3,
                Title = "Discord çalışıyor — overlay var mı?",
                Description = "Discord overlay her frame'i hook'lar. Discord ayarlarından overlay kapatmayı düşün.",
                Badge = "%3-7 FPS",
                BadgeColor = "#CC9933",
                Action = "info-discord-overlay",
            });
        }

        // 6. NVIDIA Overlay
        if (state.GeforceOverlayRunning)
        {
            actions.Add(new ActionItem
            {
                Icon = "🎯", Priority = 3,
                Title = "NVIDIA Overlay çalışıyor",
                Description = "GeForce Experience overlay FPS düşürür. Process'leri kapat.",
                Badge = "%1-3 FPS",
                BadgeColor = "#CC9933",
                Action = "close-nvidia-overlay",
            });
        }

        // 7. Genel — Shader cache (her zaman önerilebilir)
        if (actions.Count < 4)
        {
            actions.Add(new ActionItem
            {
                Icon = "🧹", Priority = 4,
                Title = "Shader Cache temizle",
                Description = "D3D, NVIDIA, AMD önbelleklerini temizle. Stutter azalır.",
                Badge = "Stutter↓",
                BadgeColor = "#3A884A",
                Action = "clear-shader-cache",
            });
        }
        if (actions.Count < 4)
        {
            actions.Add(new ActionItem
            {
                Icon = "🧹", Priority = 5,
                Title = "Windows TEMP klasörlerini temizle",
                Description = "%TEMP% + Windows\\Temp temizler. Tipik 500MB-3GB serbest.",
                Badge = "1+ GB",
                BadgeColor = "#3A884A",
                Action = "clean-temp",
            });
        }

        // En kritik 4 tanesini al, priorityye göre sırala
        var top4 = actions.OrderBy(a => a.Priority).Take(4).ToList();

        Dispatcher.Invoke(() =>
        {
            ActionList.ItemsSource = top4.Select(a => new ActionItemVm(a)).ToList();
        });
    }

    private void Action_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn) return;
        if (btn.Tag is not string action) return;

        try
        {
            switch (action)
            {
                case "disable-vbs":
                    {
                        var r = VbsToggler.Disable();
                        FoxDialog.Info(this, "VBS", r.Message + (r.RebootRequired ? "\n\n⚠ Reboot gerek." : ""));
                    } break;
                case "disable-hypervisor":
                    {
                        var r = HypervisorToggler.Disable();
                        FoxDialog.Info(this, "Hipervizor", r.Message);
                    } break;
                case "dns-and-discord":
                    {
                        // GoodbyeDPI başlat + Discord aç (en kritik)
                        var dpi = new DpiBypass();
                        var r = dpi.OpenDiscordWithBypass();
                        FoxDialog.Info(this, "Discord Erişim", r.Message);
                    } break;
                case "open-graphics-settings":
                    Process.Start(new ProcessStartInfo("ms-settings:display-advancedgraphics") { UseShellExecute = true });
                    break;
                case "info-discord-overlay":
                    FoxDialog.Info(this, "Discord Overlay",
                        "Discord'u aç → Ayarlar (sol alt ⚙) → Aktivite Ayarları → Oyun Overlay → 'Oyun Sırasında Overlay'i Aç' KAPAT.");
                    break;
                case "close-nvidia-overlay":
                    {
                        var r = OverlayDisabler.CloseNvidiaOverlay();
                        FoxDialog.Info(this, "NVIDIA Overlay", r.Message);
                    } break;
                case "clear-shader-cache":
                    {
                        var r = ShaderCacheCleaner.Clean();
                        var mb = Math.Round(r.FreedBytes / 1024.0 / 1024.0, 1);
                        FoxDialog.Success(this, "Shader Cache", $"✓ {r.DeletedFiles} dosya silindi\n✓ {mb} MB serbest");
                    } break;
                case "clean-temp":
                    {
                        var r = WindowsTweaks.CleanTempFolders();
                        FoxDialog.Success(this, "TEMP", r.Message);
                    } break;
            }
            // Buton işlemden sonra "✓ Yapıldı" durumuna geç
            btn.Content = "✓ Yapıldı";
            btn.IsEnabled = false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Onboarding action: {0}", action);
            FoxDialog.Error(this, "Hata", ex.Message);
        }
    }

    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (_page > 1) Show(_page - 1);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_page < 3) Show(_page + 1);
        else Finish();
    }

    private void Skip_Click(object sender, RoutedEventArgs e) => Finish();

    private void Finish()
    {
        _settings.HasSeenOnboarding = true;
        ProfileStore.Save(_settings);
        DialogResult = true;
        Close();
    }

    public class ActionItem
    {
        public string Icon { get; set; } = "";
        public int Priority { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Badge { get; set; } = "";
        public string BadgeColor { get; set; } = "#888";
        public string Action { get; set; } = "";
    }

    public class ActionItemVm
    {
        public ActionItem Item { get; }
        public Brush BadgeBrush { get; }
        public ActionItemVm(ActionItem item)
        {
            Item = item;
            try
            {
                var c = (Color)System.Windows.Media.ColorConverter.ConvertFromString(item.BadgeColor);
                BadgeBrush = new SolidColorBrush(c);
            }
            catch { BadgeBrush = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)); }
        }
    }
}
