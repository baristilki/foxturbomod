using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using TurboMode.Native;
using Application = System.Windows.Application;

namespace TurboMode;

public partial class App : Application
{
    // Tek instance kilidi
    private const string MutexName = @"Global\FoxTurboMod_SingleInstance_BAA8FE32";
    private static System.Threading.Mutex? _instanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        bool createdNew;
        try
        {
            _instanceMutex = new System.Threading.Mutex(true, MutexName, out createdNew);
        }
        catch (UnauthorizedAccessException)
        {
            // Global mutex erişim hatası — Local'a düş
            _instanceMutex = new System.Threading.Mutex(true,
                MutexName.Replace(@"Global\", @"Local\"), out createdNew);
        }

        if (!createdNew)
        {
            BringExistingToFront();
            Environment.Exit(0);   // Hemen ve kesin çık
            return;
        }

        DispatcherUnhandledException += OnUnhandledException;
        base.OnStartup(e);
    }

    private static void BringExistingToFront()
    {
        try
        {
            var me = Process.GetCurrentProcess();
            var others = Process.GetProcessesByName(me.ProcessName)
                .Where(p => p.Id != me.Id);
            foreach (var p in others)
            {
                if (p.MainWindowHandle != IntPtr.Zero)
                {
                    if (UserInterop.IsIconic(p.MainWindowHandle))
                        UserInterop.ShowWindow(p.MainWindowHandle, UserInterop.SW_RESTORE);
                    UserInterop.SetForegroundWindow(p.MainWindowHandle);
                    break;
                }
            }
        }
        catch { }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show(
            $"Beklenmeyen hata oluştu:\n\n{e.Exception.Message}",
            "Fox Turbo Mod",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Services.SafetyNet.RestoreIfDirty();
        // GoodbyeDPI çalışıyorsa kapanırken durdur
        try
        {
            foreach (var p in Process.GetProcessesByName("goodbyedpi"))
            {
                try { p.Kill(); } catch { }
                finally { p.Dispose(); }
            }
        }
        catch { }
        try { _instanceMutex?.ReleaseMutex(); _instanceMutex?.Dispose(); } catch { }
        base.OnExit(e);
    }
}
