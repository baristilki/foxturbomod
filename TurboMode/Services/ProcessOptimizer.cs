using System.Diagnostics;
using TurboMode.Config;
using TurboMode.Models;
using TurboMode.Native;

namespace TurboMode.Services;

/// <summary>
/// Hedef süreçleri askıya alır, sonra geri yükler. Kill ETMEZ.
/// Custom whitelist/targets'i de dikkate alır (kullanıcının kendi eklediği).
/// </summary>
public sealed class ProcessOptimizer
{
    private readonly HashSet<int> _suspendedPids = new();
    private readonly List<string> _suspendedNames = new();
    private readonly object _lock = new();
    private readonly AppSettings _settings;

    public IReadOnlyList<string> SuspendedProcessNames
    {
        get { lock (_lock) return _suspendedNames.ToArray(); }
    }

    public ProcessOptimizer(AppSettings settings)
    {
        _settings = settings;
    }

    public int Activate(string? activeGameProcess)
    {
        // Birleştirilmiş listeler: varsayılan + kullanıcı + oyun başına override
        var whitelist = new HashSet<string>(OptimizationTargets.ProcessWhitelist,
            StringComparer.OrdinalIgnoreCase);
        foreach (var w in _settings.CustomWhitelist) whitelist.Add(w);

        var targets = new HashSet<string>(OptimizationTargets.ProcessTargets,
            StringComparer.OrdinalIgnoreCase);
        foreach (var t in _settings.CustomTargets) targets.Add(t);

        // Oyun başına özel ayarlar varsa onları da uygula
        if (activeGameProcess != null &&
            _settings.GameProfiles.TryGetValue(activeGameProcess, out var profile))
        {
            foreach (var w in profile.ExtraWhitelist) whitelist.Add(w);
            foreach (var t in profile.ExtraTargets) targets.Add(t);
        }

        // Çalışan oyun sürecinin kendisini dokunma
        if (activeGameProcess != null) whitelist.Add(activeGameProcess);

        int count = 0;
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                if (whitelist.Contains(p.ProcessName)) continue;
                if (!targets.Contains(p.ProcessName)) continue;
                if (p.Id == Environment.ProcessId) continue;

                if (TrySuspend(p.Id))
                {
                    lock (_lock)
                    {
                        _suspendedPids.Add(p.Id);
                        if (!_suspendedNames.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase))
                            _suspendedNames.Add(p.ProcessName);
                    }
                    count++;
                }
            }
            catch { }
            finally { p.Dispose(); }
        }
        return count;
    }

    public void Deactivate()
    {
        int[] snapshot;
        lock (_lock)
        {
            snapshot = _suspendedPids.ToArray();
            _suspendedPids.Clear();
            _suspendedNames.Clear();
        }
        foreach (var pid in snapshot)
        {
            try { TryResume(pid); } catch { }
        }
    }

    private static bool TrySuspend(int pid)
    {
        var handle = NativeMethods.OpenProcess(
            NativeMethods.ProcessAccess.SuspendResume | NativeMethods.ProcessAccess.QueryLimitedInformation,
            false, pid);
        if (handle == IntPtr.Zero) return false;
        try { return NativeMethods.NtSuspendProcess(handle) == 0; }
        finally { NativeMethods.CloseHandle(handle); }
    }

    private static bool TryResume(int pid)
    {
        var handle = NativeMethods.OpenProcess(NativeMethods.ProcessAccess.SuspendResume, false, pid);
        if (handle == IntPtr.Zero) return false;
        try { return NativeMethods.NtResumeProcess(handle) == 0; }
        finally { NativeMethods.CloseHandle(handle); }
    }
}
