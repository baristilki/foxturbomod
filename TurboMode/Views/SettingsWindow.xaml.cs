using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TurboMode.Models;
using TurboMode.Services;
using TurboMode.Views;

namespace TurboMode.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private bool _loaded;

    public bool SettingsChanged { get; private set; }

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadSettings();
        _loaded = true;

        // Her checkbox değişikliğinde otomatik kaydet
        foreach (var cb in new[] { CbMemory, CbPower, CbGameDvr, CbPriority, CbVisual,
                                    CbCpuParking, CbMmcss, CbMouse, CbNetwork,
                                    CbFpsMonitor, CbAutoActivate, CbHotkey,
                                    OvFps, OvCpu, OvGpu, OvRam })
        {
            cb.Checked += (_, _) => Save();
            cb.Unchecked += (_, _) => Save();
        }
        foreach (var rb in new[] { OvHorizontal, OvVertical, OvSquare })
        {
            rb.Checked += (_, _) => SaveOverlayOrientation();
        }
    }

    private void SaveOverlayOrientation()
    {
        if (!_loaded) return;
        if (OvVertical.IsChecked == true) _settings.OverlayOrientation = "Vertical";
        else if (OvSquare.IsChecked == true) _settings.OverlayOrientation = "Square";
        else _settings.OverlayOrientation = "Horizontal";
        ProfileStore.Save(_settings);
        SettingsChanged = true;
    }

    private void LoadSettings()
    {
        CbMemory.IsChecked = _settings.EnableMemoryCleanup;
        CbPower.IsChecked = _settings.EnablePowerPlan;
        CbGameDvr.IsChecked = _settings.EnableGameDvrTweak;
        CbPriority.IsChecked = _settings.EnableProcessPriority;
        CbVisual.IsChecked = _settings.EnableVisualEffects;
        CbCpuParking.IsChecked = _settings.EnableCpuParking;
        CbMmcss.IsChecked = _settings.EnableSystemResponsiveness;
        CbMouse.IsChecked = _settings.EnableMouseTweak;
        CbNetwork.IsChecked = _settings.EnableNetworkOptimization;
        CbFpsMonitor.IsChecked = _settings.EnableFpsMonitor;
        CbAutoActivate.IsChecked = _settings.AutoActivateOnGameStart;
        CbHotkey.IsChecked = _settings.EnableHotkey;
        OvFps.IsChecked = _settings.OverlayShowFps;
        OvCpu.IsChecked = _settings.OverlayShowCpu;
        OvGpu.IsChecked = _settings.OverlayShowGpu;
        OvRam.IsChecked = _settings.OverlayShowRam;
        OvHorizontal.IsChecked = _settings.OverlayOrientation == "Horizontal";
        OvVertical.IsChecked = _settings.OverlayOrientation == "Vertical";
        OvSquare.IsChecked = _settings.OverlayOrientation == "Square";
        HkTurboLabel.Text = _settings.HotkeyTurbo;
        HkOverlayLabel.Text = _settings.HotkeyOverlay;
        HkLockLabel.Text = _settings.HotkeyOverlayLock;
        CbAutoStart.IsChecked = AutoStartManager.IsEnabled;
        HighlightActiveTheme();
    }

    private void AutoStart_Toggle(object sender, RoutedEventArgs e)
    {
        if (!_loaded) return;
        var ok = AutoStartManager.SetEnabled(CbAutoStart.IsChecked == true);
        if (!ok)
            FoxDialog.Warn(this, "Auto-start", "Windows başlangıcı ayarlanamadı (yetki sorunu olabilir).");
    }

    private void EditHotkey_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn) return;
        var which = btn.Tag as string ?? "";
        string title; string current;
        switch (which)
        {
            case "turbo": title = "🚀 Turbo Toggle Hotkey"; current = _settings.HotkeyTurbo; break;
            case "overlay": title = "📺 Overlay Toggle Hotkey"; current = _settings.HotkeyOverlay; break;
            case "lock": title = "🔓 Overlay Kilit Hotkey"; current = _settings.HotkeyOverlayLock; break;
            default: return;
        }

        var dlg = new HotkeyCaptureDialog(title, current) { Owner = this };
        if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.CapturedCombo))
        {
            switch (which)
            {
                case "turbo": _settings.HotkeyTurbo = dlg.CapturedCombo; HkTurboLabel.Text = dlg.CapturedCombo; break;
                case "overlay": _settings.HotkeyOverlay = dlg.CapturedCombo; HkOverlayLabel.Text = dlg.CapturedCombo; break;
                case "lock": _settings.HotkeyOverlayLock = dlg.CapturedCombo; HkLockLabel.Text = dlg.CapturedCombo; break;
            }
            ProfileStore.Save(_settings);
            SettingsChanged = true;
            FoxDialog.Info(this, "Hotkey kaydedildi",
                "Yeni hotkey aktif olması için ana pencereyi yeniden başlatın (Çıkış → tekrar aç).");
        }
    }

    private void Save()
    {
        if (!_loaded) return;
        _settings.EnableMemoryCleanup = CbMemory.IsChecked == true;
        _settings.EnablePowerPlan = CbPower.IsChecked == true;
        _settings.EnableGameDvrTweak = CbGameDvr.IsChecked == true;
        _settings.EnableProcessPriority = CbPriority.IsChecked == true;
        _settings.EnableVisualEffects = CbVisual.IsChecked == true;
        _settings.EnableCpuParking = CbCpuParking.IsChecked == true;
        _settings.EnableSystemResponsiveness = CbMmcss.IsChecked == true;
        _settings.EnableMouseTweak = CbMouse.IsChecked == true;
        _settings.EnableNetworkOptimization = CbNetwork.IsChecked == true;
        _settings.EnableFpsMonitor = CbFpsMonitor.IsChecked == true;
        _settings.AutoActivateOnGameStart = CbAutoActivate.IsChecked == true;
        _settings.EnableHotkey = CbHotkey.IsChecked == true;
        _settings.OverlayShowFps = OvFps.IsChecked == true;
        _settings.OverlayShowCpu = OvCpu.IsChecked == true;
        _settings.OverlayShowGpu = OvGpu.IsChecked == true;
        _settings.OverlayShowRam = OvRam.IsChecked == true;
        ProfileStore.Save(_settings);
        SettingsChanged = true;
    }

    private void Category_Changed(object sender, SelectionChangedEventArgs e)
    {
        // XAML parse sırasında IsSelected="True" event'i tetikler ama paneller henüz oluşmamış olabilir
        if (OptPanel == null) return;
        if (CategoryList.SelectedItem is not ListBoxItem item) return;
        OptPanel.Visibility = Visibility.Collapsed;
        NetPanel.Visibility = Visibility.Collapsed;
        FpsPanel.Visibility = Visibility.Collapsed;
        ThemePanel.Visibility = Visibility.Collapsed;
        OverlayPanel.Visibility = Visibility.Collapsed;
        HotkeyPanel.Visibility = Visibility.Collapsed;
        AdvancedPanel.Visibility = Visibility.Collapsed;

        switch (item.Tag as string)
        {
            case "opt": OptPanel.Visibility = Visibility.Visible; break;
            case "net": NetPanel.Visibility = Visibility.Visible; break;
            case "fps": FpsPanel.Visibility = Visibility.Visible; break;
            case "theme": ThemePanel.Visibility = Visibility.Visible; break;
            case "overlay": OverlayPanel.Visibility = Visibility.Visible; break;
            case "hotkey": HotkeyPanel.Visibility = Visibility.Visible; break;
            case "advanced": AdvancedPanel.Visibility = Visibility.Visible; break;
        }
    }

    private void HighlightActiveTheme()
    {
        var theme = _settings.Theme;
        // Aktif olmayan tema kartlarının opaklığını düşür
        ThemeFox.Opacity = theme == "Fox" ? 1.0 : 0.45;
        ThemeCyber.Opacity = theme == "Cyber" ? 1.0 : 0.45;
        ThemeRazer.Opacity = theme == "Razer" ? 1.0 : 0.45;
    }

    private void ThemeFox_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => SetTheme("Fox");
    private void ThemeCyber_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => SetTheme("Cyber");
    private void ThemeRazer_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => SetTheme("Razer");

    private void SetTheme(string theme)
    {
        _settings.Theme = theme;
        ProfileStore.Save(_settings);
        SettingsChanged = true;
        HighlightActiveTheme();
        ThemeManager.Apply(ThemeManager.Parse(theme));
        ThemeNote.Text = $"'{theme}' tema kaydedildi. Tam görünüm için uygulamayı yeniden başlat.";
    }

    private void OpenSettingsFolder_Click(object sender, RoutedEventArgs e)
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FoxMod");
        Directory.CreateDirectory(dir);
        Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
    }

    private void OpenLogFile_Click(object sender, RoutedEventArgs e)
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FoxMod", "logs");
        Directory.CreateDirectory(dir);
        Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
    }

    private void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        if (!FoxDialog.Confirm(this, "Geçmişi Temizle",
            "Tüm oyun geçmişi silinecek. Emin misin?")) return;
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FoxMod", "history.json");
        try { if (File.Exists(path)) File.Delete(path); }
        catch (Exception ex) { Log.Error(ex, "Geçmiş silinemedi"); }
    }

    private void ResetSettings_Click(object sender, RoutedEventArgs e)
    {
        if (!FoxDialog.Confirm(this, "Sıfırla",
            "TÜM ayarlar varsayılana döner (beyaz liste dahil). Emin misin?")) return;
        var fresh = new AppSettings();
        ProfileStore.Save(fresh);
        LoadSettings();
        SettingsChanged = true;
    }
}
