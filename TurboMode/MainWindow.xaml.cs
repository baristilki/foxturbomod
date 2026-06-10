using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using TurboMode.Models;
using TurboMode.Services;
using TurboMode.ViewModels;
using TurboMode.Views;

namespace TurboMode;

public partial class MainWindow : Window
{
    private readonly TrayIconHost _tray = new();
    private readonly HotkeyManager _hotkeys = new();
    private bool _reallyExit;

    public MainWindow()
    {
        InitializeComponent();
        _tray.Initialize(this, "/Resources/FoxMod.ico", "Fox Turbo Mod", BuildTrayMenu);

        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _tray.ShowBalloon("Fox Turbo Mod", "Arka planda çalışmaya devam ediyor.");
            }
        };

        Loaded += (_, _) =>
        {
            try
            {
                // Windows başlangıcında açıldıysa pencere tepsiye in (auto-start kullanıcı kararı)
                if (Services.AutoStartManager.IsEnabled && DataContext is MainViewModel vAuto
                    && vAuto.Settings.StartMinimized && vAuto.Settings.HasSeenOnboarding)
                {
                    Hide();
                    _tray.ShowBalloon("Fox Turbo Mod", "Arka planda çalışmaya başladı. Tepsiden açabilirsin.");
                }

                // İlk açılışta onboarding turu
                if (DataContext is MainViewModel v && !v.Settings.HasSeenOnboarding)
                {
                    var ob = new OnboardingWindow(v.Settings) { Owner = this };
                    ob.ShowDialog();
                }

                _hotkeys.Attach(this);
                if (DataContext is MainViewModel vmGraph)
                {
                    LiveFpsGraph.Bind(vmGraph.Fps);
                }
                if (DataContext is MainViewModel vm)
                {
                    RegisterAllHotkeys(vm);
                }
            }
            catch (Exception ex) { Log.Error(ex, "Hotkey kayıt hatası"); }
        };
    }

    private System.Collections.Generic.List<(string label, Action action)> BuildTrayMenu()
    {
        var menu = new System.Collections.Generic.List<(string, Action)>();
        if (DataContext is MainViewModel vm)
        {
            menu.Add(("Pencereyi Aç", () => { Show(); WindowState = WindowState.Normal; Activate(); }));
            menu.Add(("──", () => { }));
            menu.Add((vm.TurboEnabled ? "✓ Turbo Kapat" : "🚀 Turbo Aç", () => vm.TurboEnabled = !vm.TurboEnabled));
            menu.Add(("📊 Geçmiş", () => OpenWindow<HistoryWindow>()));
            menu.Add(("🚀 Öneriler", () => OpenWindow<RecommendationsWindow>()));
            menu.Add(("⚙ Ayarlar", () => OpenSettingsWindow()));
            menu.Add(("──", () => { }));
            menu.Add(("Çıkış", () => { _reallyExit = true; Close(); }));
        }
        return menu;
    }

    private void OpenWindow<T>() where T : Window, new()
    {
        try
        {
            Show(); WindowState = WindowState.Normal; Activate();
            var win = new T { Owner = this };
            win.ShowDialog();
        }
        catch (Exception ex) { Log.Error(ex, "{0} açılamadı", typeof(T).Name); }
    }

    private void OpenSettingsWindow()
    {
        if (DataContext is not MainViewModel vm) return;
        try
        {
            Show(); WindowState = WindowState.Normal; Activate();
            var win = new SettingsWindow(vm.Settings) { Owner = this };
            win.ShowDialog();
            if (win.SettingsChanged)
            {
                vm.ReloadSettings();
                RegisterAllHotkeys(vm);
            }
        }
        catch (Exception ex) { Log.Error(ex, "Settings açılamadı"); }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_reallyExit)
        {
            try { _overlay?.Close(); _overlay = null; } catch { }
            _hotkeys.Dispose();
            _tray.Dispose();
            base.OnClosing(e);
            return;
        }

        if (DataContext is not MainViewModel vm)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        var pref = vm.Settings.CloseAction;
        if (pref == "Tray")
        {
            e.Cancel = true;
            Hide();
            return;
        }
        if (pref == "Exit")
        {
            _reallyExit = true;
            _hotkeys.Dispose();
            _tray.Dispose();
            base.OnClosing(e);
            return;
        }

        var dlg = new CloseDialog { Owner = this };
        dlg.ShowDialog();
        switch (dlg.ChosenAction)
        {
            case CloseDialog.Action.Cancel:
                e.Cancel = true;
                return;
            case CloseDialog.Action.MinimizeToTray:
                if (dlg.RememberChoice) { vm.Settings.CloseAction = "Tray"; ProfileStore.Save(vm.Settings); }
                e.Cancel = true;
                Hide();
                return;
            case CloseDialog.Action.FullExit:
                if (dlg.RememberChoice) { vm.Settings.CloseAction = "Exit"; ProfileStore.Save(vm.Settings); }
                _reallyExit = true;
                _hotkeys.Dispose();
                _tray.Dispose();
                base.OnClosing(e);
                return;
        }
    }

    private void Whitelist_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        var win = new WhitelistWindow(vm.Settings) { Owner = this };
        if (win.ShowDialog() == true) vm.ReloadSettings();
    }

    private void History_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var win = new HistoryWindow { Owner = this };
            win.ShowDialog();
        }
        catch (Exception ex) { Log.Error(ex, "Geçmiş açılamadı"); }
    }

    private void Recommendations_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var win = new RecommendationsWindow { Owner = this };
            win.ShowDialog();
        }
        catch (Exception ex) { Log.Error(ex, "Öneriler açılamadı"); }
    }

    private void Settings_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow();

    private void RegisterAllHotkeys(MainViewModel vm)
    {
        try { _hotkeys.UnregisterAll(); } catch { }
        if (!vm.Settings.EnableHotkey) return;

        // Turbo toggle
        var (m, k) = HotkeyManager.Parse(vm.Settings.HotkeyTurbo);
        if (k > 0)
        {
            _hotkeys.Register(m, k, () =>
            {
                vm.TurboEnabled = !vm.TurboEnabled;
                _tray.ShowBalloon("Fox Turbo Mod",
                    vm.TurboEnabled ? "🚀 Turbo AÇILDI" : "✓ Turbo kapatıldı");
            });
            Log.Info($"Hotkey: Turbo = {vm.Settings.HotkeyTurbo}");
        }

        // Overlay toggle
        (m, k) = HotkeyManager.Parse(vm.Settings.HotkeyOverlay);
        if (k > 0)
        {
            _hotkeys.Register(m, k, () => Overlay_Click(this, new RoutedEventArgs()));
            Log.Info($"Hotkey: Overlay = {vm.Settings.HotkeyOverlay}");
        }

        // Overlay lock
        (m, k) = HotkeyManager.Parse(vm.Settings.HotkeyOverlayLock);
        if (k > 0)
        {
            _hotkeys.Register(m, k, () =>
            {
                if (_overlay != null && _overlay.IsVisible)
                {
                    _overlay.ToggleLockFromOutside();
                    _tray.ShowBalloon("Fox Turbo Mod",
                        _overlay.IsLockedNow ? "🔒 Overlay kilitlendi" : "🔓 Overlay kilidi açıldı");
                }
            });
            Log.Info($"Hotkey: Overlay Lock = {vm.Settings.HotkeyOverlayLock}");
        }
    }

    private OverlayWindow? _overlay;
    private void Overlay_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        try
        {
            if (_overlay == null || !_overlay.IsVisible)
            {
                _overlay = new OverlayWindow(vm);
                _overlay.Closed += (_, _) => { _overlay = null; vm.OverlayActive = false; };
                _overlay.Show();
                vm.OverlayActive = true;
                _tray.ShowBalloon("Fox Turbo Mod", "Overlay açıldı — taşımak için doğrudan sürükle, 🔒 ile kilitle");
            }
            else
            {
                _overlay.Close();
                _overlay = null;
                vm.OverlayActive = false;
            }
        }
        catch (Exception ex) { Log.Error(ex, "Overlay açılamadı"); }
    }


    private void ShowOnboarding_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        try
        {
            var ob = new OnboardingWindow(vm.Settings) { Owner = this };
            ob.ShowDialog();
        }
        catch (Exception ex) { Log.Error(ex, "Onboarding açılamadı"); }
    }

    private void Hogs_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        try
        {
            var win = new ResourceHogsWindow(vm.Hogs) { Owner = this };
            win.ShowDialog();
        }
        catch (Exception ex) { Log.Error(ex, "Resource hogs açılamadı"); }
    }

    private void SuspendedDetails_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        var procs = vm.LastSuspendedProcesses;
        var svcs = vm.LastStoppedServices;
        if (procs.Count == 0 && svcs.Count == 0)
        {
            Views.FoxDialog.Info(this, "Donduruldu",
                "Henüz hiçbir süreç/servis askıya alınmadı.");
            return;
        }
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"🧊 Bu seansta askıya alınanlar ({procs.Count} süreç + {svcs.Count} servis):");
        sb.AppendLine();
        if (procs.Count > 0)
        {
            sb.AppendLine("SÜREÇLER:");
            foreach (var p in procs.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine($"  • {p}");
            sb.AppendLine();
        }
        if (svcs.Count > 0)
        {
            sb.AppendLine("SERVİSLER:");
            foreach (var s in svcs.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine($"  • {s}");
        }
        sb.AppendLine();
        sb.AppendLine("Oyun kapanınca hepsi otomatik geri açılır.");
        Views.FoxDialog.Info(this, "Donduruldu — Detaylar", sb.ToString());
    }

    private void Discord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.Clipboard.SetText("peacefox");
            try { Process.Start(new ProcessStartInfo("https://discord.com/") { UseShellExecute = true }); }
            catch { }
            _tray.ShowBalloon("Fox Turbo Mod", "Discord kullanıcı adı 'peacefox' panoya kopyalandı.");
        }
        catch (Exception ex) { Log.Error(ex, "Discord click"); }
    }

    private void GitHub_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo(UpdateChecker.RepoUrl) { UseShellExecute = true }); }
        catch (Exception ex) { Log.Error(ex, "GitHub click"); }
    }

    private void Game_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not Models.InstalledGame g) return;
        try
        {
            Process.Start(new ProcessStartInfo(g.ExecutablePath)
            {
                UseShellExecute = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(g.ExecutablePath)
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Oyun başlatılamadı: {0}", g.DisplayName);
            Views.FoxDialog.Info(this, "Oyun Başlatılamadı",
                $"{g.DisplayName} başlatılamadı:\n\n{ex.Message}\n\n" +
                "Bu oyun launcher üzerinden açılmak zorunda olabilir (Steam, Epic, Riot Client).");
        }
    }

    private void OpenUpdate_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (vm.PendingUpdate == null) return;
        var win = new UpdateWindow(vm.PendingUpdate) { Owner = this };
        win.ShowDialog();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _reallyExit = true;
        Close();
    }
}
