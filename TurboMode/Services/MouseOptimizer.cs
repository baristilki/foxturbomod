using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// Windows "Enhance pointer precision" (mouse acceleration) kapatır.
/// Aim training ve competitive shooters için klasik tweak.
/// Geri yüklemede eski değerleri restore eder.
/// </summary>
public sealed class MouseOptimizer
{
    private const string MousePath = @"Control Panel\Mouse";
    private object? _prevSpeed, _prevT1, _prevT2;

    public bool Activate()
    {
        try
        {
            using var k = Registry.CurrentUser.OpenSubKey(MousePath, true);
            if (k == null) return false;

            _prevSpeed = k.GetValue("MouseSpeed");
            _prevT1 = k.GetValue("MouseThreshold1");
            _prevT2 = k.GetValue("MouseThreshold2");

            // 0 = acceleration off (1:1 mouse hareketi)
            k.SetValue("MouseSpeed", "0", RegistryValueKind.String);
            k.SetValue("MouseThreshold1", "0", RegistryValueKind.String);
            k.SetValue("MouseThreshold2", "0", RegistryValueKind.String);
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        try
        {
            using var k = Registry.CurrentUser.OpenSubKey(MousePath, true);
            if (k == null) return;
            if (_prevSpeed != null) k.SetValue("MouseSpeed", _prevSpeed);
            if (_prevT1 != null) k.SetValue("MouseThreshold1", _prevT1);
            if (_prevT2 != null) k.SetValue("MouseThreshold2", _prevT2);
        }
        catch { }
        _prevSpeed = _prevT1 = _prevT2 = null;
    }
}
