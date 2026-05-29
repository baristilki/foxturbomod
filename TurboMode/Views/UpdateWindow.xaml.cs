using System.Windows;
using TurboMode.Services;

namespace TurboMode.Views;

public partial class UpdateWindow : Window
{
    private readonly UpdateChecker.UpdateInfo _info;
    private readonly AutoUpdater _updater = new();
    private bool _updating;

    public UpdateWindow(UpdateChecker.UpdateInfo info)
    {
        InitializeComponent();
        _info = info;
        VersionLabel.Text = $"v{info.CurrentVersion} → v{info.LatestVersion}  •  " +
                            $"{Math.Round(info.DownloadSize / 1024.0 / 1024.0, 1)} MB";

        _updater.StatusChanged += s =>
            Dispatcher.Invoke(() => StatusLabel.Text = s);
        _updater.ProgressChanged += p =>
            Dispatcher.Invoke(() => ProgressBar.Value = p * 100);

        if (string.IsNullOrEmpty(info.DownloadUrl))
        {
            StatusLabel.Text = "Release sayfasında FoxTurboMod.exe bulunamadı. Manuel indirin.";
            ActionButton.Content = "GitHub'da Aç";
        }
    }

    private async void Action_Click(object sender, RoutedEventArgs e)
    {
        if (_updating) return;

        // Asset yoksa GitHub'a yönlendir
        if (string.IsNullOrEmpty(_info.DownloadUrl))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_info.ReleaseUrl)
                { UseShellExecute = true });
            }
            catch { }
            Close();
            return;
        }

        _updating = true;
        ActionButton.IsEnabled = false;
        CancelButton.IsEnabled = false;

        var success = await _updater.DownloadAndApplyAsync(_info.DownloadUrl, _info.DownloadSize);
        if (success)
        {
            StatusLabel.Text = "Hazır. Uygulama kapanıp yeniden başlatılacak.";
            await Task.Delay(800);
            System.Windows.Application.Current.Shutdown();
        }
        else
        {
            ActionButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            _updating = false;
            ActionButton.Content = "Tekrar Dene";
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (_updating) return;
        Close();
    }
}
