using System.IO;

namespace TurboMode.Services;

public static class ShaderCacheCleaner
{
    public sealed record CleanResult(int DeletedFiles, long FreedBytes, List<string> Errors);

    private static readonly string[] CachePaths =
    {
        @"%LOCALAPPDATA%\D3DSCache",
        @"%LOCALAPPDATA%\NVIDIA\DXCache",
        @"%LOCALAPPDATA%\NVIDIA\GLCache",
        @"%LOCALAPPDATA%\AMD\DxCache",
        @"%LOCALAPPDATA%\AMD\DxcCache",
    };

    public static CleanResult Clean()
    {
        int deleted = 0;
        long freed = 0;
        var errors = new List<string>();

        foreach (var raw in CachePaths)
        {
            var path = Environment.ExpandEnvironmentVariables(raw);
            if (!Directory.Exists(path)) continue;
            try
            {
                foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var len = new FileInfo(f).Length;
                        File.Delete(f);
                        deleted++;
                        freed += len;
                    }
                    catch { /* dosya kilitli, geç */ }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(path)}: {ex.Message}");
            }
        }
        return new CleanResult(deleted, freed, errors);
    }
}
