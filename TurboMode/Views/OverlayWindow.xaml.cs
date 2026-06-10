using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using TurboMode.Services;
using TurboMode.ViewModels;
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;

namespace TurboMode.Views;

public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_TOOLWINDOW = 0x80;

    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    private readonly MainViewModel _vm;
    private readonly DispatcherTimer _timer;
    private bool _locked;

    public OverlayWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;

        // Kaydedilmiş konumu yükle
        Left = vm.Settings.OverlayLeft;
        Top = vm.Settings.OverlayTop;
        _locked = vm.Settings.OverlayLocked;

        SourceInitialized += (_, _) => ApplyLockState();
        MouseLeftButtonDown += (_, e) =>
        {
            if (!_locked) try { DragMove(); } catch { }
        };

        // Sürükleme bitince konumu kaydet
        LocationChanged += (_, _) => SaveLocation();

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();
        Refresh();

        // İlk açılışta lock ikonunu güncel state'e ayarla
        LockIcon.Text = _locked ? "🔒" : "🔓";
    }

    private void SaveLocation()
    {
        try
        {
            _vm.Settings.OverlayLeft = Left;
            _vm.Settings.OverlayTop = Top;
            _vm.Settings.OverlayLocked = _locked;
            ProfileStore.Save(_vm.Settings);
        }
        catch (Exception ex) { Log.Error(ex, "Overlay konum kaydedilemedi"); }
    }

    private void ApplyLockState()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;
        var ex = GetWindowLong(hwnd, GWL_EXSTYLE);
        if (_locked)
            SetWindowLong(hwnd, GWL_EXSTYLE, ex | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        else
            SetWindowLong(hwnd, GWL_EXSTYLE, (ex & ~WS_EX_TRANSPARENT) | WS_EX_TOOLWINDOW);
    }

    public bool IsLockedNow => _locked;

    private void CloseOverlay_Click(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        Close();
    }

    public void ToggleLockFromOutside()
    {
        _locked = !_locked;
        ApplyLockState();
        LockIcon.Text = _locked ? "🔒" : "🔓";
        Log.Info("Overlay lock (hotkey): " + (_locked ? "LOCKED" : "UNLOCKED"));
        SaveLocation();
    }

    private void LockIcon_PreviewDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void ToggleLock_Click(object sender, MouseButtonEventArgs e)
    {
        _locked = !_locked;
        ApplyLockState();
        LockIcon.Text = _locked ? "🔒" : "🔓";
        LockIcon.ToolTip = _locked
            ? "Kilitli — Ctrl+Alt+L ile aç (click-through aktif)"
            : "Açık — sürükleyebilirsin, tıkla kilitle (veya Ctrl+Alt+L)";
        Log.Info("Overlay lock: " + (_locked ? "LOCKED" : "UNLOCKED"));
        SaveLocation();
        e.Handled = true;
    }

    private static readonly Color ActiveGreen = Color.FromRgb(0x44, 0xD6, 0x2C);
    private static readonly Color IdleGray = Color.FromRgb(0x55, 0x55, 0x55);

    private void Refresh()
    {
        // Her refresh'te settings'i tazele — kullanıcı Ayarlar penceresinden değişiklik yapmış olabilir
        var settings = ProfileStore.Load();

        // Visibility — kapalıysa hem block hem ondan sonraki ayraç gizle
        FpsBlock.Visibility = settings.OverlayShowFps ? Visibility.Visible : Visibility.Collapsed;
        Sep2.Visibility = settings.OverlayShowFps ? Visibility.Visible : Visibility.Collapsed;

        CpuBlock.Visibility = settings.OverlayShowCpu ? Visibility.Visible : Visibility.Collapsed;
        Sep3.Visibility = settings.OverlayShowCpu ? Visibility.Visible : Visibility.Collapsed;

        GpuBlock.Visibility = settings.OverlayShowGpu ? Visibility.Visible : Visibility.Collapsed;
        Sep4.Visibility = settings.OverlayShowGpu ? Visibility.Visible : Visibility.Collapsed;

        RamBlock.Visibility = settings.OverlayShowRam ? Visibility.Visible : Visibility.Collapsed;
        Sep5.Visibility = settings.OverlayShowRam ? Visibility.Visible : Visibility.Collapsed;

        // Yerleşim — yatay / dikey
        var newOrientation = settings.OverlayOrientation switch
        {
            "Vertical" => System.Windows.Controls.Orientation.Vertical,
            _ => System.Windows.Controls.Orientation.Horizontal,
        };
        if (RootPanel.Orientation != newOrientation)
        {
            RootPanel.Orientation = newOrientation;
            // Dikey modda ayraçları yatay çizgi yap
            if (newOrientation == System.Windows.Controls.Orientation.Vertical)
            {
                Sep1.Height = 1; Sep1.Width = double.NaN; Sep1.Margin = new Thickness(8, 4, 8, 4);
                Sep2.Height = 1; Sep2.Width = double.NaN; Sep2.Margin = new Thickness(8, 4, 8, 4);
                Sep3.Height = 1; Sep3.Width = double.NaN; Sep3.Margin = new Thickness(8, 4, 8, 4);
                Sep4.Height = 1; Sep4.Width = double.NaN; Sep4.Margin = new Thickness(8, 4, 8, 4);
                Sep5.Height = 1; Sep5.Width = double.NaN; Sep5.Margin = new Thickness(8, 4, 8, 4);
            }
            else
            {
                Sep1.Width = 1; Sep1.Height = double.NaN; Sep1.Margin = new Thickness(0, 4, 0, 4);
                Sep2.Width = 1; Sep2.Height = double.NaN; Sep2.Margin = new Thickness(0, 4, 0, 4);
                Sep3.Width = 1; Sep3.Height = double.NaN; Sep3.Margin = new Thickness(0, 4, 0, 4);
                Sep4.Width = 1; Sep4.Height = double.NaN; Sep4.Margin = new Thickness(0, 4, 0, 4);
                Sep5.Width = 1; Sep5.Height = double.NaN; Sep5.Margin = new Thickness(0, 4, 0, 4);
            }
        }

        var fps = _vm.Fps.AverageFps(3);
        FpsValue.Text = fps > 0 ? $"{fps:0}" : "—";

        RamValue.Text = _vm.RamLabel.Replace(" / ", "/").Replace(" GB", "G");

        CpuTempValue.Text = _vm.CpuTempLabel == "—" ? "—" : _vm.CpuTempLabel;
        CpuTempValue.Foreground = _vm.CpuTempColor;
        CpuLoadValue.Text = $"{_vm.CpuLoad:0}%";

        GpuTempValue.Text = _vm.GpuTempLabel == "—" ? "—" : _vm.GpuTempLabel;
        GpuTempValue.Foreground = _vm.GpuTempColor;
        GpuLoadValue.Text = $"{_vm.GpuLoad:0}%";

        if (_vm.TurboEnabled)
        {
            TurboDot.Fill = new SolidColorBrush(ActiveGreen);
            if (DotGlow != null)
            {
                DotGlow.Color = ActiveGreen;
                DotGlow.BlurRadius = 12;
                DotGlow.Opacity = 0.9;
            }
        }
        else
        {
            TurboDot.Fill = new SolidColorBrush(IdleGray);
            if (DotGlow != null)
            {
                DotGlow.Color = IdleGray;
                DotGlow.BlurRadius = 0;
                DotGlow.Opacity = 0;
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveLocation();
        _timer.Stop();
        base.OnClosed(e);
    }
}
