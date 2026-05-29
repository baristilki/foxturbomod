using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace TurboMode.Services;

/// <summary>
/// GitHub releases endpoint'inden son sürümü kontrol eder.
/// Yeni varsa bildirim için UpdateAvailable event'i tetikler.
/// Repo bilgisi sabit; fork edenler RepoOwner/RepoName'i değiştirir.
/// </summary>
public static class UpdateChecker
{
    public const string RepoOwner = "baristilki";
    public const string RepoName = "foxturbomod";
    public static string RepoUrl => $"https://github.com/{RepoOwner}/{RepoName}";
    public static string ReleasesUrl => $"{RepoUrl}/releases";

    public sealed record UpdateInfo(string LatestVersion, string CurrentVersion, string ReleaseUrl, bool IsNewer);

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

            var current = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
            var latest = tag.TrimStart('v', 'V');

            bool isNewer = CompareVersions(latest, current) > 0;
            return new UpdateInfo(latest, current, htmlUrl, isNewer);
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
