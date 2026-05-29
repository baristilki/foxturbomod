using System.Diagnostics;

namespace TurboMode.Services;

/// <summary>
/// Aktif oyun sürecinin CPU önceliğini "High" yapar (Realtime DEĞİL — o sistemi dondurur).
/// Deaktivasyonda Normal'a döndürür. Oyun zaten kapandıysa bir şey yapmaz.
/// </summary>
public sealed class ProcessPriorityOptimizer
{
    private int _trackedPid = -1;
    private ProcessPriorityClass _previous = ProcessPriorityClass.Normal;

    public bool Activate(string? gameProcessName)
    {
        if (string.IsNullOrEmpty(gameProcessName)) return false;

        try
        {
            var procs = Process.GetProcessesByName(gameProcessName);
            if (procs.Length == 0) return false;
            var target = procs[0];
            _previous = target.PriorityClass;
            target.PriorityClass = ProcessPriorityClass.High;
            _trackedPid = target.Id;
            foreach (var p in procs) p.Dispose();
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        if (_trackedPid < 0) return;
        try
        {
            using var p = Process.GetProcessById(_trackedPid);
            p.PriorityClass = _previous;
        }
        catch { /* oyun zaten kapanmış olabilir */ }
        _trackedPid = -1;
    }
}
