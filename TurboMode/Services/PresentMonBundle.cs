using System.IO;
using System.Reflection;

namespace TurboMode.Services;

/// <summary>
/// PresentMon.exe binary'sini exe'nin içine gömülü olarak taşır;
/// ilk kullanımda %LOCALAPPDATA%\FoxMod\PresentMon.exe olarak extract eder.
/// </summary>
public static class PresentMonBundle
{
    public static string ExtractedPath { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FoxMod", "PresentMon.exe");

    public static bool EnsureExtracted()
    {
        try
        {
            var dir = Path.GetDirectoryName(ExtractedPath)!;
            Directory.CreateDirectory(dir);
            if (File.Exists(ExtractedPath) && new FileInfo(ExtractedPath).Length > 100_000)
                return true;

            var asm = Assembly.GetExecutingAssembly();
            // LogicalName "PresentMon.exe" — csproj'da bu adla embed edildi
            using var stream = asm.GetManifestResourceStream("PresentMon.exe");
            if (stream == null) return false;
            using var file = File.Create(ExtractedPath);
            stream.CopyTo(file);
            return true;
        }
        catch { return false; }
    }
}
