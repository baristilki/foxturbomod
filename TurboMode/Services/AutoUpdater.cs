using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace TurboMode.Services;

/// <summary>
/// Yeni FoxTurboMod.exe'yi indirir, mevcut exe ile değiştirir, yeniden başlatır.
///
/// Sorun: Exe çalışırken kendi dosyasına yazılamaz.
/// Çözüm: Yeni exe yan dosya olarak indirilir, bir batch script üretilir,
/// uygulama kapatılır, batch script eski exe'yi siler + yeniyi rename eder + başlatır + kendini siler.
/// </summary>
public sealed class AutoUpdater
{
    public event Action<double>? ProgressChanged;   // 0..1 download ilerlemesi
    public event Action<string>? StatusChanged;     // "Bağlanılıyor...", "İndiriliyor...", "Kuruluyor..."

    public async Task<bool> DownloadAndApplyAsync(string downloadUrl, long expectedSize)
    {
        var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(currentExe) || !File.Exists(currentExe))
        {
            StatusChanged?.Invoke("Mevcut exe yolu bulunamadı.");
            return false;
        }

        var dir = Path.GetDirectoryName(currentExe)!;
        var newExePath = Path.Combine(dir, "FoxTurboMod_new.exe");
        var updateScript = Path.Combine(dir, "_update.cmd");

        try
        {
            StatusChanged?.Invoke("Bağlanılıyor...");
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("FoxTurboMod-AutoUpdater/1.0");

            using var resp = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!resp.IsSuccessStatusCode)
            {
                StatusChanged?.Invoke($"HTTP hatası: {(int)resp.StatusCode}");
                return false;
            }

            var total = resp.Content.Headers.ContentLength ?? expectedSize;
            StatusChanged?.Invoke("İndiriliyor...");

            using (var input = await resp.Content.ReadAsStreamAsync())
            using (var output = File.Create(newExePath))
            {
                var buffer = new byte[81920];
                long downloaded = 0;
                int read;
                while ((read = await input.ReadAsync(buffer)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, read));
                    downloaded += read;
                    if (total > 0) ProgressChanged?.Invoke((double)downloaded / total);
                }
            }

            // Boyut sağlık kontrolü
            var actualSize = new FileInfo(newExePath).Length;
            if (actualSize < 1_000_000)  // 1MB altı şüpheli
            {
                File.Delete(newExePath);
                StatusChanged?.Invoke("İndirilen dosya çok küçük, iptal edildi.");
                return false;
            }

            StatusChanged?.Invoke("Kuruluyor...");
            // Update batch script — uygulama kapandıktan sonra dosyaları değiştirip yeniyi başlatır
            var currentExeName = Path.GetFileName(currentExe);
            var script =
                "@echo off\r\n" +
                "timeout /t 2 /nobreak >nul\r\n" +
                $":retry\r\n" +
                $"del \"{currentExeName}\" >nul 2>&1\r\n" +
                $"if exist \"{currentExeName}\" (timeout /t 1 /nobreak >nul & goto retry)\r\n" +
                $"move /Y \"FoxTurboMod_new.exe\" \"{currentExeName}\" >nul\r\n" +
                $"start \"\" \"{currentExeName}\"\r\n" +
                "(goto) 2>nul & del \"%~f0\"\r\n";

            await File.WriteAllTextAsync(updateScript, script);

            // Batch'i fire-and-forget olarak başlat
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{updateScript}\"\"",
                WorkingDirectory = dir,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            Process.Start(psi);

            StatusChanged?.Invoke("Uygulama yeniden başlatılıyor...");
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Hata: {ex.Message}");
            try { if (File.Exists(newExePath)) File.Delete(newExePath); } catch { }
            try { if (File.Exists(updateScript)) File.Delete(updateScript); } catch { }
            return false;
        }
    }
}
