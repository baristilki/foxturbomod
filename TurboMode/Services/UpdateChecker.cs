using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace TurboMode.Services;

public static class UpdateChecker
{
    public const string RepoOwner = "baristilki";
    public const string RepoName = "foxturbomod";
    public static string RepoUrl => $"https://github.com/{RepoOwner}/{RepoName}";
    public static string ReleasesUrl => $"{RepoUrl}/releases";

    public sealed record UpdateInfo(
        string LatestVersion,
        string CurrentVersion,
        string ReleaseUrl,
        string? DownloadUrl,
        long DownloadSize,
        bool IsNewer);

    public static async Task<UpdateInfo?> CheckAsync()
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("FoxTurboMod-UpdateCheck/1.0");
            http.Timeout = TimeSpan.FromSeconds(8);

            var url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
            var htmlUrl = doc.RootElement.GetProperty("html_url").GetString() ?? "";

            // FoxTurboMod.exe asset'ini bul
            string? downloadUrl = null;
            long downloadSize = 0;
            if (doc.RootElement.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString();
                    if (name != null && name.Equals("FoxTurboMod.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        downloadSize = asset.GetProperty("size").GetInt64();
                        break;
                    }
                }
            }

            var current = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
            var latest = tag.TrimStart('v', 'V');
            bool isNewer = CompareVersions(latest, current) > 0;

            return new UpdateInfo(latest, current, htmlUrl, downloadUrl, downloadSize, isNewer);
        }
        catch { return null; }
    }

    private static int CompareVersions(string a, string b)
    {
        try
        {
            var va = new Version(NormalizeVersion(a));
            var vb = new Version(NormalizeVersion(b));
            return va.CompareTo(vb);
        }
        catch { return 0; }
    }

    private static string NormalizeVersion(string s)
    {
        var parts = s.Split('.', '-', '+');
        var nums = new List<string>();
        foreach (var p in parts)
        {
            if (int.TryParse(p, out _)) nums.Add(p);
            if (nums.Count >= 3) break;
        }
        while (nums.Count < 3) nums.Add("0");
        return string.Join('.', nums);
    }
}
