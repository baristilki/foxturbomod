using System.Diagnostics;
using System.Globalization;

namespace TurboMode.Services;

/// <summary>
/// PresentMon kullanarak gerçek zamanlı FPS ölçer.
/// PresentMon ETW (Event Tracing for Windows) ile DXGI Present çağrılarını dinler;
/// her frame için bir CSV satırı stdout'a yazar.
/// Biz son N saniyenin ortalamasını tutarız ve baseline/active karşılaştırması yaparız.
/// </summary>
public sealed class FpsMonitor : IDisposable
{
    public event Action<double>? FpsUpdated;

    private Process? _process;
    private readonly object _lock = new();
    // (timestamp utc, ms between presents)
    private readonly LinkedList<(DateTime At, double FrameMs)> _samples = new();
    private const int MaxHistorySeconds = 90;
    private string? _watchedProcess;

    public bool IsRunning => _process != null && !_process.HasExited;
    public string? WatchedProcess => _watchedProcess;

    public bool Start(string gameProcessName)
    {
        if (IsRunning) Stop();
        if (!PresentMonBundle.EnsureExtracted()) return false;

        _watchedProcess = gameProcessName;
        // PresentMon v2: argümanlar değişti.
        // -process_name X.exe -output_stdout -terminate_existing_session
        // ÖNEMLİ: process adı boşluk içerebilir (League of Legends.exe), tırnak ŞART
        // Adım 1: Eski ETW session'ları temizle (PresentMon ayrı çağrılır, hiç capture yapmaz)
        try
        {
            var cleanup = new ProcessStartInfo(PresentMonBundle.ExtractedPath, "--terminate_existing_session")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var c = Process.Start(cleanup);
            c?.WaitForExit(2000);
            Log.Info("PresentMon eski session temizlendi");
        }
        catch (Exception ex) { Log.Warn("Cleanup hatası: {0}", ex.Message); }

        // Adım 2: Capture başlat — sadece process_name + output_stdout
        var args = $"--process_name \"{gameProcessName}.exe\" --output_stdout";

        try
        {
            Log.Info("FpsMonitor: PresentMon başlatılıyor → {0} {1}", PresentMonBundle.ExtractedPath, args);
            _process = Process.Start(new ProcessStartInfo(PresentMonBundle.ExtractedPath, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
            });
            if (_process == null) { Log.Warn("FpsMonitor: process null"); return false; }
            Log.Info("FpsMonitor: PresentMon PID={0}", _process.Id);

            _ = Task.Run(ReadLoop);
            _ = Task.Run(async () =>
            {
                // PresentMon stderr — hata mesajları (PID bulunamadı vs.)
                try
                {
                    while (!_process.HasExited)
                    {
                        var line = await _process.StandardError.ReadLineAsync();
                        if (line == null) break;
                        Log.Warn("PresentMon stderr: {0}", line);
                    }
                }
                catch { }
            });
            return true;
        }
        catch (Exception ex) { Log.Error(ex, "FpsMonitor start hatası"); return false; }
    }

    private async Task ReadLoop()
    {
        if (_process == null) return;
        var sr = _process.StandardOutput;
        bool headerSkipped = false;
        int frameMsCol = -1;
        int linesRead = 0;

        while (!_process.HasExited)
        {
            string? line = await sr.ReadLineAsync();
            if (line == null) break;
            if (line.Length == 0) continue;
            linesRead++;

            if (!headerSkipped)
            {
                Log.Info("PresentMon header: {0}", line);
                // PresentMon v2: "MsBetweenPresents" veya "TimeInSeconds" sütunları var
                var cols = line.Split(',').Select(c => c.Trim()).ToArray();
                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i].Equals("MsBetweenPresents", StringComparison.OrdinalIgnoreCase) ||
                        cols[i].Equals("msBetweenPresents", StringComparison.OrdinalIgnoreCase) ||
                        cols[i].Equals("FrameTime", StringComparison.OrdinalIgnoreCase) ||
                        cols[i].Equals("CPUDuration", StringComparison.OrdinalIgnoreCase))
                    {
                        frameMsCol = i;
                        Log.Info("FpsMonitor: frame time sütun bulundu: {0} (index {1})", cols[i], i);
                        break;
                    }
                }
                if (frameMsCol < 0)
                    Log.Warn("FpsMonitor: frame time sütun bulunamadı. Sütunlar: {0}", string.Join(",", cols));
                headerSkipped = true;
                continue;
            }

            if (frameMsCol < 0) continue;
            var parts = line.Split(',');
            if (parts.Length <= frameMsCol) continue;
            if (!double.TryParse(parts[frameMsCol], NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
                continue;
            if (ms <= 0 || ms > 1000) continue;

            var now = DateTime.UtcNow;
            int totalSamples;
            lock (_lock)
            {
                _samples.AddLast((now, ms));
                while (_samples.Count > 0 &&
                       (now - _samples.First!.Value.At).TotalSeconds > MaxHistorySeconds)
                    _samples.RemoveFirst();
                totalSamples = _samples.Count;
            }
            // Her 300. sample'da bir log
            if (totalSamples % 300 == 0)
                Log.Info($"FPS sample #{totalSamples}: frameMs={ms:0.00}, instant FPS={1000.0/ms:0.0}, avg(3s)={AverageFps(3):0.0}");
            FpsUpdated?.Invoke(1000.0 / ms);
        }
    }

    /// <summary>
    /// Son N saniyenin ortalama FPS'i. Yeterli sample yoksa 0 döner.
    /// </summary>
    public double AverageFps(int seconds)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-seconds);
        double sum = 0; int count = 0;
        lock (_lock)
        {
            foreach (var s in _samples)
            {
                if (s.At < cutoff) continue;
                sum += s.FrameMs;
                count++;
            }
        }
        if (count < 30) return 0; // yetersiz veri
        return 1000.0 / (sum / count);
    }

    public int SampleCount
    {
        get { lock (_lock) return _samples.Count; }
    }

    public void Stop()
    {
        try
        {
            if (_process != null && !_process.HasExited) _process.Kill(entireProcessTree: true);
            _process?.Dispose();
        }
        catch { }
        _process = null;
        _watchedProcess = null;
        lock (_lock) _samples.Clear();
    }

    public void Dispose() => Stop();
}
