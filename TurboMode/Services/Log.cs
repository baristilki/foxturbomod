using System.IO;

namespace TurboMode.Services;

/// <summary>
/// Basit dosya tabanlı logger — Serilog yerine bağımlılıksız yazıldı.
/// %LOCALAPPDATA%\FoxMod\logs\foxmod-YYYYMMDD.log dosyasına yazar.
/// </summary>
public static class Log
{
    private static readonly object _lock = new();
    private static readonly string LogDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FoxMod", "logs");

    private static string CurrentLogFile =>
        Path.Combine(LogDir, $"foxmod-{DateTime.Now:yyyyMMdd}.log");

    static Log()
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            // 7 günden eski log dosyalarını sil
            foreach (var f in Directory.GetFiles(LogDir, "foxmod-*.log"))
            {
                try
                {
                    if (File.GetLastWriteTime(f) < DateTime.Now.AddDays(-7))
                        File.Delete(f);
                }
                catch { }
            }
        }
        catch { }
    }

    public static void Info(string message, params object?[] args) =>
        Write("INF", message, null, args);

    public static void Warn(string message, params object?[] args) =>
        Write("WRN", message, null, args);

    public static void Error(Exception? ex, string message, params object?[] args) =>
        Write("ERR", message, ex, args);

    private static void Write(string level, string message, Exception? ex, object?[] args)
    {
        try
        {
            // Basit {0}, {1} veya {Name} substitution
            for (int i = 0; i < args.Length; i++)
            {
                message = message.Replace("{" + i + "}", args[i]?.ToString() ?? "null");
            }
            // Named placeholder'ları kalanları args ile sırayla ikame et
            int argIdx = 0;
            while (message.Contains('{') && message.Contains('}') && argIdx < args.Length)
            {
                var start = message.IndexOf('{');
                var end = message.IndexOf('}', start);
                if (end <= start) break;
                var placeholder = message.Substring(start, end - start + 1);
                // Sadece {Word} formatındaki placeholder'ları değiştir
                if (placeholder.Length > 2 && char.IsLetter(placeholder[1]))
                {
                    message = message.Replace(placeholder, args[argIdx]?.ToString() ?? "null");
                    argIdx++;
                }
                else break;
            }

            var line = $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}";
            if (ex != null) line += $"\n    Exception: {ex.GetType().Name}: {ex.Message}\n    Stack: {ex.StackTrace}";

            lock (_lock)
            {
                File.AppendAllText(CurrentLogFile, line + Environment.NewLine);
            }
        }
        catch { }
    }
}
