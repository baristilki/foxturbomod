using System.Diagnostics;
using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// Windows üzerinde "tek-tıkla" optimizasyonlar.
/// Hepsi geri alınabilir ama dönüş için kullanıcı manuel ayarlardan veya
/// Recommendations penceresinde "Geri al" desteği eklenince yapılır.
/// </summary>
public static class WindowsTweaks
{
    public sealed record Result(bool Success, string Message);

    /// <summary>
    /// Microsoft Store / UWP arka plan uygulamalarını topluca kapatır.
    /// HKCU\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications\GlobalUserDisabled = 1
    /// </summary>
    public static Result DisableBackgroundApps()
    {
        try
        {
            using var k1 = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", true);
            k1?.SetValue("GlobalUserDisabled", 1, RegistryValueKind.DWord);

            using var k2 = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Search", true);
            k2?.SetValue("BackgroundAppGlobalToggle", 0, RegistryValueKind.DWord);

            return new Result(true, "Microsoft Store uygulamalarının arka plan izinleri kapatıldı. " +
                "Etki anında. Tekrar açmak için Ayarlar → Gizlilik → Arka Plan Uygulamaları.");
        }
        catch (Exception ex) { return new Result(false, ex.Message); }
    }

    /// <summary>
    /// CPU/disk yiyen Windows telemetri ve uyumluluk task'larını devre dışı bırakır.
    /// </summary>
    public static Result DisableTelemetryTasks()
    {
        var tasks = new[]
        {
            @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
            @"\Microsoft\Windows\Application Experience\ProgramDataUpdater",
            @"\Microsoft\Windows\Application Experience\StartupAppTask",
            @"\Microsoft\Windows\Application Experience\PcaPatchDbTask",
            @"\Microsoft\Windows\Autochk\Proxy",
            @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
            @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
            @"\Microsoft\Windows\Feedback\Siuf\DmClient",
            @"\Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload",
            @"\Microsoft\Windows\Windows Error Reporting\QueueReporting",
            @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector",
        };

        int disabled = 0;
        foreach (var t in tasks)
        {
            try
            {
                var psi = new ProcessStartInfo("schtasks.exe", $"/Change /TN \"{t}\" /DISABLE")
                {
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true,
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(3000);
                if (p?.ExitCode == 0) disabled++;
            }
            catch { }
        }
        return new Result(true,
            $"{disabled} zamanlanmış görev devre dışı bırakıldı. " +
            "Tekrar açmak için: schtasks /Change /TN <name> /ENABLE.");
    }

    /// <summary>
    /// %TEMP% ve C:\Windows\Temp klasörlerini temizler.
    /// Kullanımdaki dosyalar atlanır (sıfır risk).
    /// </summary>
    public static Result CleanTempFolders()
    {
        var paths = new[]
        {
            Environment.GetEnvironmentVariable("TEMP") ?? "",
            Environment.GetEnvironmentVariable("TMP") ?? "",
            @"C:\Windows\Temp",
        }.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToArray();

        int deletedFiles = 0;
        long freedBytes = 0;
        var errors = new List<string>();
        foreach (var path in paths)
        {
            if (!System.IO.Directory.Exists(path)) continue;
            try
            {
                foreach (var f in System.IO.Directory.EnumerateFiles(path, "*", System.IO.SearchOption.AllDirectories))
                {
                    try
                    {
                        var len = new System.IO.FileInfo(f).Length;
                        System.IO.File.Delete(f);
                        deletedFiles++;
                        freedBytes += len;
                    }
                    catch { /* kullanımda - geç */ }
                }
                // Boş klasörleri de temizle
                foreach (var d in System.IO.Directory.EnumerateDirectories(path, "*", System.IO.SearchOption.AllDirectories).Reverse())
                {
                    try { if (!System.IO.Directory.EnumerateFileSystemEntries(d).Any()) System.IO.Directory.Delete(d); } catch { }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{System.IO.Path.GetFileName(path)}: {ex.Message}");
            }
        }
        var mb = Math.Round(freedBytes / 1024.0 / 1024.0, 1);
        var msg = $"✓ {deletedFiles} dosya silindi\n✓ {mb} MB serbest";
        if (errors.Count > 0) msg += $"\n\nUyarılar:\n{string.Join("\n", errors.Take(3))}";
        return new Result(true, msg);
    }

    /// <summary>
    /// NVIDIA Control Panel'i açar (varsa).
    /// </summary>
    public static Result OpenNvidiaControlPanel()
    {
        try
        {
            var paths = new[]
            {
                @"C:\Program Files\NVIDIA Corporation\Control Panel Client\nvcplui.exe",
                @"C:\Windows\System32\nvcplui.exe",
            };
            foreach (var p in paths)
            {
                if (System.IO.File.Exists(p))
                {
                    Process.Start(new ProcessStartInfo(p) { UseShellExecute = true });
                    return new Result(true, "NVIDIA Control Panel açılıyor.");
                }
            }
            // Fallback: Store'da NVIDIA Control Panel
            Process.Start(new ProcessStartInfo("nvcplui.exe") { UseShellExecute = true });
            return new Result(true, "Açılıyor...");
        }
        catch
        {
            return new Result(false, "NVIDIA Control Panel bulunamadı. NVIDIA sürücüsü kurulu mu?");
        }
    }
}
