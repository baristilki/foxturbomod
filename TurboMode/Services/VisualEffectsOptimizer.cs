using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// Windows görsel efektlerini geçici olarak "En İyi Performans" moduna alır.
/// Pencere animasyonları, gölgeler, şeffaflık kapanır. %2-3 FPS kazancı + UI snappier.
/// </summary>
public sealed class VisualEffectsOptimizer
{
    private object? _previousVfx;
    private const string VfxKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects";
    // VisualFXSetting: 0=auto, 1=appearance, 2=performance, 3=custom

    public bool Activate()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(VfxKey, true);
            if (key == null) return false;
            _previousVfx = key.GetValue("VisualFXSetting");
            key.SetValue("VisualFXSetting", 2, RegistryValueKind.DWord);
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        if (_previousVfx == null) return;
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(VfxKey, true);
            if (key == null) return;
            key.SetValue("VisualFXSetting", _previousVfx, RegistryValueKind.DWord);
        }
        catch { }
        _previousVfx = null;
    }
}
