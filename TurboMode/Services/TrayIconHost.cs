using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace TurboMode.Services;

/// <summary>
/// Windows tepsi ikonu — pencere kapanınca/minimize edilince tepsiye gider.
/// WinForms NotifyIcon kullanıyoruz (ek paket gerekmez, .NET 8 ile gelir).
/// </summary>
public sealed class TrayIconHost : IDisposable
{
    private NotifyIcon? _icon;
    private Window? _mainWindow;

    public void Initialize(Window mainWindow, string iconResourcePath, string tooltip)
    {
        _mainWindow = mainWindow;

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

        var menu = new ContextMenuStrip();
        menu.Items.Add("Aç", null, (_, _) => ShowWindow());
        menu.Items.Add("-");
        menu.Items.Add("Çıkış", null, (_, _) => ExitApp());
        _icon.ContextMenuStrip = menu;
        _icon.DoubleClick += (_, _) => ShowWindow();
    }

    private void ShowWindow()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApp()
    {
        Dispose();
        Application.Current.Shutdown();
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
