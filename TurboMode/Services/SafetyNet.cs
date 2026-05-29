using System.IO;

namespace TurboMode.Services;

/// <summary>
/// Eğer Turbo aktifken uygulama çökerse, kullanıcının arka plan uygulamaları/servisleri
/// donmuş halde kalır. SafetyNet, %LOCALAPPDATA%\TurboMode\dirty.lock dosyası ile bunu işaret eder.
/// Bir sonraki açılışta MainViewModel ya da App.OnExit bu dosyayı görürse askıdaki süreçleri
/// "all processes resume" yapar (best effort). Çok agresif olmamak için sadece bizim hedef
/// listesindeki süreçlere dokunuruz.
/// </summary>
public static class SafetyNet
{
    private static readonly string DirtyFile =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FoxMod",
            "dirty.lock");

    public static void MarkDirty()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DirtyFile)!);
            File.WriteAllText(DirtyFile, DateTime.UtcNow.ToString("o"));
        }
        catch { }
    }

    public static void ClearDirty()
    {
        try { if (File.Exists(DirtyFile)) File.Delete(DirtyFile); } catch { }
    }

    /// <summary>
    /// Eğer dirty flag varsa, bilinen tüm hedef süreçler için resume çağrısı yapar.
    /// Sonra bayrağı temizler. Hedef listesi dışındaki süreçlere DOKUNMAZ.
    /// </summary>
    public static void RestoreIfDirty()
    {
        try
        {
            if (!File.Exists(DirtyFile)) return;

            // Best-effort: hedef listesindeki tüm process'leri resume etmeyi dener.
            // Askıda değilse ntdll resume çağrısı no-op gibi davranır.
            foreach (var p in System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (Array.Exists(Config.OptimizationTargets.ProcessTargets,
                        n => string.Equals(n, p.ProcessName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var handle = Native.NativeMethods.OpenProcess(
                            Native.NativeMethods.ProcessAccess.SuspendResume, false, p.Id);
                        if (handle != IntPtr.Zero)
                        {
                            Native.NativeMethods.NtResumeProcess(handle);
                            Native.NativeMethods.CloseHandle(handle);
                        }
                    }
                }
                catch { }
                finally { p.Dispose(); }
            }

            ClearDirty();
        }
        catch { }
    }
}
