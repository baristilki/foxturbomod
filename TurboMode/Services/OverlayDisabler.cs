using System.Diagnostics;

namespace TurboMode.Services;

/// <summary>
/// Discord ve NVIDIA Overlay process'lerini kapatır.
/// Discord overlay'i programatik kapatmanın güvenilir yolu yok — Discord'u tamamen kapatıyoruz.
/// NVIDIA Overlay process'leri otomatik yeniden başlamaz (GeForce Experience kapatılırsa kalıcı).
/// </summary>
public static class OverlayDisabler
{
    public sealed record Result(bool Success, string Message, int KilledCount);

    public static Result CloseDiscord()
    {
        int killed = KillByName(new[]
        {
            "Discord", "DiscordCanary", "DiscordPTB", "DiscordDevelopment",
            "Discord Helper", "Discord Helper (Renderer)", "Discord Helper (GPU)"
        });
        return killed > 0
            ? new Result(true, $"Discord kapatıldı ({killed} process). Overlay artık aktif değil.", killed)
            : new Result(false, "Discord zaten çalışmıyordu.", 0);
    }

    public static Result CloseNvidiaOverlay()
    {
        // NVIDIA Share, NVIDIA Overlay, NVIDIA Container child'ları (overlay özelinde)
        int killed = KillByName(new[]
        {
            "NVIDIA Overlay", "NVIDIA Share", "NVIDIA Web Helper",
            "NVIDIA GeForce Experience", "NVIDIA GeForce Overlay",
        });
        return killed > 0
            ? new Result(true,
                $"NVIDIA Overlay process'leri kapatıldı ({killed} adet). " +
                "Bilgisayar yeniden başlatıldığında geri açılır; kalıcı için GeForce Experience ayarlarından kapat.", killed)
            : new Result(false, "NVIDIA Overlay process'i zaten çalışmıyordu.", 0);
    }

    private static int KillByName(string[] names)
    {
        int total = 0;
        foreach (var n in names)
        {
            try
            {
                foreach (var p in Process.GetProcessesByName(n))
                {
                    try { p.Kill(entireProcessTree: true); total++; }
                    catch { }
                    finally { p.Dispose(); }
                }
            }
            catch { }
        }
        return total;
    }
}
