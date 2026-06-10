using System.Net.NetworkInformation;

namespace TurboMode.Services;

public static class NetworkProbe
{
    public sealed record PingResult(string Target, long PingMs, bool Success);

    public static async Task<List<PingResult>> ProbeAsync()
    {
        var targets = new[]
        {
            ("Cloudflare DNS", "1.1.1.1"),
            ("Google DNS", "8.8.8.8"),
            ("Discord", "discord.com"),
            ("Steam", "steamcommunity.com"),
        };

        var results = new List<PingResult>();
        foreach (var (label, host) in targets)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, 2000);
                results.Add(new PingResult(label,
                    reply.Status == IPStatus.Success ? reply.RoundtripTime : -1,
                    reply.Status == IPStatus.Success));
            }
            catch
            {
                results.Add(new PingResult(label, -1, false));
            }
        }
        return results;
    }
}
