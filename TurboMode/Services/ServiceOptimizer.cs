using System.ServiceProcess;
using TurboMode.Config;

namespace TurboMode.Services;

/// <summary>
/// Windows servislerini geçici olarak durdurur, sonra eski state'ine döndürür.
/// ÖNEMLİ: Servisin önceki durumunu kaydederiz; aktifken durdurduysak sadece o zaman geri başlatırız.
/// Zaten durdurulmuş bir servis bizim yüzümüzden açılmaz.
/// </summary>
public sealed class ServiceOptimizer
{
    private readonly Dictionary<string, ServiceControllerStatus> _previousStatus = new();
    private readonly HashSet<string> _blacklist = new(
        OptimizationTargets.ServiceBlacklist, StringComparer.OrdinalIgnoreCase);

    public int Activate()
    {
        int stopped = 0;
        foreach (var t in OptimizationTargets.ServiceTargets)
        {
            if (_blacklist.Contains(t.ServiceName)) continue;
            try
            {
                using var sc = new ServiceController(t.ServiceName);
                var prev = sc.Status;
                if (prev == ServiceControllerStatus.Running || prev == ServiceControllerStatus.StartPending)
                {
                    _previousStatus[t.ServiceName] = prev;
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                    stopped++;
                }
            }
            catch
            {
                // Servis yoksa veya yetki yetersizse atla. Activate genelinde başarısız olmamalı.
            }
        }
        return stopped;
    }

    public void Deactivate()
    {
        foreach (var kv in _previousStatus)
        {
            try
            {
                using var sc = new ServiceController(kv.Key);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(8));
                }
            }
            catch
            {
                // Restore best-effort. Servis yine de bir sonraki açılışta normal şekilde başlar.
            }
        }
        _previousStatus.Clear();
    }
}
