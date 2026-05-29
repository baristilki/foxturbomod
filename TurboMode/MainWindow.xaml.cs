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
    private bool _reallyExit;

    public MainWindow()
    {
        InitializeComponent();
        _tray.Initialize(this, "/Resources/FoxMod.ico", "Fox Turbo Mod");

        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _tray.ShowBalloon("Fox Turbo Mod", "Arka planda çalışmaya devam ediyor.");
            }
        };
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_reallyExit)
        {
            _tray.Dispose();
            base.OnClosing(e);
            return;
        }

        if (DataContext is not MainViewModel vm)
        {
            // ViewModel yoksa varsayılan: tepsiye al
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
            _tray.Dispose();
            base.OnClosing(e);
            return;
        }

        // Ask — diyalog göster
        var dlg = new CloseDialog { Owner = this };
        dlg.ShowDialog();
        switch (dlg.ChosenAction)
        {
            case CloseDialog.Action.Cancel:
                e.Cancel = true;
                return;
            case CloseDialog.Action.MinimizeToTray:
                if (dlg.RememberChoice) { vm.Settings.CloseAction = "Tray"; Services.ProfileStore.Save(vm.Settings); }
                e.Cancel = true;
                Hide();
                return;
            case CloseDialog.Action.FullExit:
                if (dlg.RememberChoice) { vm.Settings.CloseAction = "Exit"; Services.ProfileStore.Save(vm.Settings); }
                _reallyExit = true;
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
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Geçmiş açılamadı:\n\n" + ex,
                "Fox Turbo Mod", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void SuspendedDetails_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        var procs = vm.LastSuspendedProcesses;
        var svcs = vm.LastStoppedServices;
        if (procs.Count == 0 && svcs.Count == 0)
        {
            System.Windows.MessageBox.Show(
                "Henüz hiçbir süreç/servis askıya alınmadı.",
                "Donduruldu", MessageBoxButton.OK, MessageBoxImage.Information);
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

        System.Windows.MessageBox.Show(sb.ToString(),
            "Donduruldu — Detaylar",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Recommendations_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var win = new RecommendationsWindow { Owner = this };
            win.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Öneriler açılamadı:\n\n" + ex,
                "Fox Turbo Mod", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Discord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.Clipboard.SetText("peacefox");
            try { Process.Start(new ProcessStartInfo("https://discord.com/") { UseShellExecute = true }); }
            catch { }
            _tray.ShowBalloon("Fox Turbo Mod",
                "Discord kullanıcı adı 'peacefox' panoya kopyalandı.");
        }
        catch { }
    }

    private void GitHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(UpdateChecker.RepoUrl) { UseShellExecute = true });
        }
        catch { }
    }

    private void Game_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not InstalledGame g) return;
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
            System.Windows.MessageBox.Show(
                $"{g.DisplayName} başlatılamadı:\n\n{ex.Message}\n\n" +
                "Bu oyun launcher üzerinden açılmak zorunda olabilir (Steam, Epic, Riot Client).",
                "Fox Turbo Mod", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenUpdate_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
