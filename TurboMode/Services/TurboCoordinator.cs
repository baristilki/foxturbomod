using TurboMode.Models;

namespace TurboMode.Services;

public record ActivationResult(
    int SuspendedProcesses,
    int StoppedServices,
    long RamFreedMb,
    bool PowerPlanSwitched,
    bool NetworkOptimized,
    bool GameDvrDisabled,
    bool PriorityBoosted,
    bool VisualEffectsMinimized,
    bool CpuParkingDisabled,
    bool MmcssBoosted,
    bool MouseTweaked,
    IReadOnlyList<string> SuspendedProcessNames,
    IReadOnlyList<string> StoppedServiceNames);

public sealed class TurboCoordinator
{
    private readonly AppSettings _settings;
    private readonly ProcessOptimizer _proc;
    private readonly ServiceOptimizer _svc = new();
    private readonly PowerPlanOptimizer _pwr = new();
    private readonly NetworkOptimizer _net = new();
    private readonly GameDvrOptimizer _dvr = new();
    private readonly ProcessPriorityOptimizer _pri = new();
    private readonly VisualEffectsOptimizer _vfx = new();
    private readonly CpuParkingOptimizer _park = new();
    private readonly SystemResponsivenessOptimizer _mmcss = new();
    private readonly MouseOptimizer _mouse = new();
    private bool _isActive;

    public bool IsActive => _isActive;

    public TurboCoordinator(AppSettings settings)
    {
        _settings = settings;
        _proc = new ProcessOptimizer(settings);
    }

    public ActivationResult Activate(string? gameProcess, string? gameExePath)
    {
        if (_isActive)
            return new ActivationResult(0, 0, 0, false, false, false, false, false, false, false, false,
                Array.Empty<string>(), Array.Empty<string>());
        _isActive = true;
        SafetyNet.MarkDirty();

        int suspended = _proc.Activate(gameProcess);
        int stopped = _svc.Activate();
        bool powerSwitched = _settings.EnablePowerPlan && _pwr.Activate();
        long ramFreedMb = _settings.EnableMemoryCleanup ? MemoryOptimizer.PurgeStandbyList() : 0;
        bool netOpt = _settings.EnableNetworkOptimization && _net.Activate(gameExePath);
        bool dvrOff = _settings.EnableGameDvrTweak && _dvr.Activate();
        bool prio = _settings.EnableProcessPriority && _pri.Activate(gameProcess);
        bool vfx = _settings.EnableVisualEffects && _vfx.Activate();
        bool parking = _settings.EnableCpuParking && _park.Activate();
        bool mmcss = _settings.EnableSystemResponsiveness && _mmcss.Activate();
        bool mouse = _settings.EnableMouseTweak && _mouse.Activate();

        return new ActivationResult(suspended, stopped, ramFreedMb,
            powerSwitched, netOpt, dvrOff, prio, vfx, parking, mmcss, mouse,
            _proc.SuspendedProcessNames, _svc.StoppedServiceNames);
    }

    public void Deactivate()
    {
        if (!_isActive) return;
        _isActive = false;

        _mouse.Deactivate();
        _mmcss.Deactivate();
        _park.Deactivate();
        _vfx.Deactivate();
        _pri.Deactivate();
        _dvr.Deactivate();
        _net.Deactivate();
        _pwr.Deactivate();
        _svc.Deactivate();
        _proc.Deactivate();

        SafetyNet.ClearDirty();
    }
}
