using System.Diagnostics;
using System.IO;

namespace TurboMode.Services;

/// <summary>
/// Aktif ağ adaptörlerinin DNS sunucusunu Cloudflare/Google'a değiştirir.
/// DNS-based engellemeleri bypass eder + ortalama %30 daha hızlı DNS çözümlemesi.
/// </summary>
public static class DnsOptimizer
{
    public sealed record Result(bool Success, string Message);

    public static Result SwitchToCloudflare()
    {
        // Cloudflare (1.1.1.1, 1.0.0.1) primary + Google (8.8.8.8) fallback
        var ps =
            "$adapters = Get-NetAdapter | Where-Object { $_.Status -eq 'Up' -and $_.HardwareInterface -eq $true }; " +
            "foreach ($a in $adapters) { " +
            "  Set-DnsClientServerAddress -InterfaceIndex $a.ifIndex -ServerAddresses ('1.1.1.1','1.0.0.1','8.8.8.8') -ErrorAction SilentlyContinue " +
            "}; " +
            "Clear-DnsClientCache; " +
            "ipconfig /flushdns | Out-Null";
        var (ok, err) = RunPs(ps);
        return ok
            ? new Result(true,
                "DNS Cloudflare (1.1.1.1) olarak ayarlandı.\n" +
                "DNS cache temizlendi.\n\n" +
                "Test: tarayıcıdan https://discord.com aç — açılıyorsa engelleme DNS bazlıydı, çözüldü.\n" +
                "Açılmıyorsa ISP'in TLS SNI inspection yapıyor — VPN gerekebilir.")
            : new Result(false, "DNS değiştirilemedi: " + err);
    }

    public static Result RestoreAutomatic()
    {
        var ps =
            "$adapters = Get-NetAdapter | Where-Object { $_.Status -eq 'Up' -and $_.HardwareInterface -eq $true }; " +
            "foreach ($a in $adapters) { " +
            "  Set-DnsClientServerAddress -InterfaceIndex $a.ifIndex -ResetServerAddresses -ErrorAction SilentlyContinue " +
            "}; " +
            "Clear-DnsClientCache; " +
            "ipconfig /flushdns | Out-Null";
        var (ok, err) = RunPs(ps);
        return ok
            ? new Result(true, "DNS otomatik (ISP) ayarlarına döndü.")
            : new Result(false, "Geri yüklenemedi: " + err);
    }

    /// <summary>
    /// DNS'i Cloudflare yap + Discord'u başlat.
    /// </summary>
    public static Result OpenDiscordWithDns()
    {
        var dns = SwitchToCloudflare();
        if (!dns.Success) return dns;

        var path = FindDiscordPath();
        if (path == null)
            return new Result(true,
                "DNS değiştirildi. Discord kurulu değil görünüyor — discord.com/download'dan kurabilirsin.");

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return new Result(true,
                "DNS değiştirildi ve Discord başlatıldı.\n\n" +
                "Açılmazsa: ISP'in TLS SNI inspection yapıyor olabilir, VPN gerekecek.");
        }
        catch (Exception ex)
        {
            return new Result(true, $"DNS değişti ama Discord başlatılamadı: {ex.Message}");
        }
    }

    private static string? FindDiscordPath()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var updater = Path.Combine(local, "Discord", "Update.exe");
        if (File.Exists(updater)) return updater;

        // Direkt exe arayışı — en yeni app-* klasörü
        var discordRoot = Path.Combine(local, "Discord");
        if (Directory.Exists(discordRoot))
        {
            var appDirs = Directory.GetDirectories(discordRoot, "app-*");
            if (appDirs.Length > 0)
            {
                Array.Sort(appDirs);
                var exe = Path.Combine(appDirs[^1], "Discord.exe");
                if (File.Exists(exe)) return exe;
            }
        }
        return null;
    }

    private static (bool ok, string err) RunPs(string script)
    {
        try
        {
            var psi = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{script.Replace("\"", "`\"")}\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            if (p == null) return (false, "PowerShell başlatılamadı");
            string err = p.StandardError.ReadToEnd();
            p.WaitForExit(15000);
            return (p.ExitCode == 0, err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
