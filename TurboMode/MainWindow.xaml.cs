using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
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
        if (!_reallyExit)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        _tray.Dispose();
        base.OnClosing(e);
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

    private void OpenUpdate_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (string.IsNullOrEmpty(vm.UpdateUrl)) return;
        try { Process.Start(new ProcessStartInfo(vm.UpdateUrl) { UseShellExecute = true }); }
        catch { }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _reallyExit = true;
        Close();
    }
}
