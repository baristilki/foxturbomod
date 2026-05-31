using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TurboMode.Services;

/// <summary>
/// GoodbyeDPI (ValdikSS, MIT) — TLS Client Hello fragmentation ile DPI engellemesini atlatır.
/// WinDivert (WFP) driver kullanır. Yönetici yetkisi gerek.
///
/// Türkiye'deki Discord/sosyal medya engellemesi için yaygın yöntem.
/// Ek not: Valorant/Vanguard ile birlikte çalışmayabilir — oyun açmadan önce Stop().
/// </summary>
public sealed class DpiBypass : IDisposable
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FoxMod", "GoodbyeDPI");

    private static string ExePath => Path.Combine(Dir, "goodbyedpi.exe");
    private static string DllPath => Path.Combine(Dir, "WinDivert.dll");
    private static string SysPath => Path.Combine(Dir, "WinDivert64.sys");

    private Process? _process;

    public bool IsRunning => _process != null && !_process.HasExited;

    public sealed record Result(bool Success, string Message);

    public Result Start()
    {
        if (IsRunning) return new Result(true, "GoodbyeDPI zaten çalışıyor.");
        if (!Extract()) return new Result(false, "GoodbyeDPI dosyaları çıkartılamadı.");

        try
        {
            // -9 modu: Türkiye ve benzeri ülkeler için en yaygın çalışan ayar.
            // TLS Client Hello fragmentation + DNS redirect + max-payload.
            var args = "-9 --dns-addr 1.1.1.1 --dns-port 53 --dnsv6-addr 2606:4700:4700::1111 --dnsv6-port 53";
            _process = Process.Start(new ProcessStartInfo(ExePath, args)
            {
                WorkingDirectory = Dir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            if (_process == null) return new Result(false, "GoodbyeDPI başlatılamadı.");
            return new Result(true,
                "GoodbyeDPI aktif. Şimdi Discord/engellenmiş siteleri test et.\n\n" +
                "⚠ Valorant (Vanguard) açacaksan önce 'GoodbyeDPI'i Durdur' butonuna bas — " +
                "kernel driver çakışması olabilir.");
        }
        catch (Exception ex)
        {
            return new Result(false, "Hata: " + ex.Message);
        }
    }

    public Result Stop()
    {
        if (!IsRunning) return new Result(true, "GoodbyeDPI zaten kapalı.");
        try
        {
            _process!.Kill(entireProcessTree: true);
            _process.Dispose();
            _process = null;
            return new Result(true, "GoodbyeDPI durduruldu.");
        }
        catch (Exception ex) { return new Result(false, ex.Message); }
    }

    public Result OpenDiscordWithBypass()
    {
        var s = Start();
        if (!s.Success) return s;

        // 2 saniye bekle — WinDivert driver yüklensin
        Thread.Sleep(2000);

        var path = FindDiscord();
        if (path == null)
            return new Result(true,
                "GoodbyeDPI aktif. Discord kurulu değil — discord.com/download'dan indir.");

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return new Result(true,
                "GoodbyeDPI aktif + Discord başlatıldı.\n\n" +
                "Çalışmazsa: Discord'u tamamen kapat (tepside Discord ikonu sağ tık → Quit Discord), " +
                "sonra tekrar dene. ⚠ Valorant açmadan önce GoodbyeDPI'i durdur.");
        }
        catch (Exception ex)
        {
            return new Result(true, $"GoodbyeDPI çalışıyor, Discord başlatılamadı: {ex.Message}");
        }
    }

    private static bool Extract()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            ExtractResource("goodbyedpi.exe", ExePath);
            ExtractResource("WinDivert.dll", DllPath);
            ExtractResource("WinDivert64.sys", SysPath);
            return true;
        }
        catch { return false; }
    }

    private static void ExtractResource(string name, string target)
    {
        if (File.Exists(target) && new FileInfo(target).Length > 1000) return;
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource yok: {name}");
        using var file = File.Create(target);
        stream.CopyTo(file);
    }

    private static string? FindDiscord()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var updater = Path.Combine(local, "Discord", "Update.exe");
        if (File.Exists(updater)) return updater;

        var discordRoot = Path.Combine(local, "Discord");
        if (Directory.Exists(discordRoot))
        {
            var dirs = Directory.GetDirectories(discordRoot, "app-*");
            if (dirs.Length > 0)
            {
                Array.Sort(dirs);
                var exe = Path.Combine(dirs[^1], "Discord.exe");
                if (File.Exists(exe)) return exe;
            }
        }
        return null;
    }

    public void Dispose() => Stop();
}
