using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace TurboMode;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnUnhandledException;
        base.OnStartup(e);
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show(
            $"Beklenmeyen hata oluştu:\n\n{e.Exception.Message}",
            "Fox Mod",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Eğer Turbo Mode aktifken uygulama kapanırsa, snapshot'tan geri yükleme yap.
        Services.SafetyNet.RestoreIfDirty();
        base.OnExit(e);
    }
}
