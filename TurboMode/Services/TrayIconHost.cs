using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace TurboMode.Services;

public sealed class TrayIconHost : IDisposable
{
    private NotifyIcon? _icon;
    private Window? _mainWindow;
    private Func<List<(string label, Action action)>>? _menuBuilder;

    public void Initialize(Window mainWindow, string iconResourcePath, string tooltip,
        Func<List<(string label, Action action)>>? menuBuilder = null)
    {
        _mainWindow = mainWindow;
        _menuBuilder = menuBuilder;

        _icon = new NotifyIcon
        {
            Visible = true,
            Text = tooltip,
        };

        try
        {
            var info = Application.GetResourceStream(new Uri(iconResourcePath, UriKind.Relative));
            if (info != null) _icon.Icon = new Icon(info.Stream);
        }
        catch
        {
            _icon.Icon = SystemIcons.Application;
        }

        _icon.MouseUp += (_, e) =>
        {
            if (e.Button == MouseButtons.Right)
                ShowMenu();
        };
        _icon.DoubleClick += (_, _) => ShowWindow();
    }

    private void ShowMenu()
    {
        if (_menuBuilder == null)
        {
            // Eski basit menü
            var basic = new ContextMenuStrip();
            basic.Items.Add("Aç", null, (_, _) => ShowWindow());
            basic.Items.Add("Çıkış", null, (_, _) => Application.Current.Shutdown());
            _icon!.ContextMenuStrip = basic;
            return;
        }

        try
        {
            var menu = new ContextMenuStrip();
            foreach (var (label, action) in _menuBuilder())
            {
                if (label == "──") menu.Items.Add(new ToolStripSeparator());
                else
                {
                    var captured = action;
                    menu.Items.Add(label, null, (_, _) => captured());
                }
            }
            _icon!.ContextMenuStrip = menu;
            _icon.ContextMenuStrip.Show(Control.MousePosition);
        }
        catch (Exception ex) { Log.Error(ex, "Tray menü göster hatası"); }
    }

    private void ShowWindow()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    public void ShowBalloon(string title, string text)
    {
        if (_icon == null) return;
        _icon.BalloonTipTitle = title;
        _icon.BalloonTipText = text;
        _icon.ShowBalloonTip(3000);
    }

    public void Dispose()
    {
        if (_icon != null)
        {
            _icon.Visible = false;
            _icon.Dispose();
            _icon = null;
        }
    }
}
