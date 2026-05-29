using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// Xbox Game Bar / Game DVR kayıt servisini geçici kapatır.
/// Sürekli arka planda son 30 saniyeyi kaydeden bu özellik %1-3 FPS yer; oyun bitince geri açılır.
/// </summary>
public sealed class GameDvrOptimizer
{
    private record Snapshot(string Hive, string Path, string Name, object? Value, RegistryValueKind Kind);
    private readonly List<Snapshot> _previous = new();

    private static readonly (string Hive, string Path, string Name)[] Targets =
    {
        ("HKCU", @"System\GameConfigStore", "GameDVR_Enabled"),
        ("HKCU", @"System\GameConfigStore", "GameDVR_FSEBehaviorMode"),
        ("HKCU", @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled"),
        ("HKCU", @"Software\Microsoft\GameBar", "AutoGameModeEnabled"),
        ("HKCU", @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled"),
    };

    public bool Activate()
    {
        _previous.Clear();
        try
        {
            foreach (var t in Targets)
            {
                var key = OpenKey(t.Hive, t.Path, writable: true);
                if (key == null) continue;
                var existing = key.GetValue(t.Name);
                var kind = existing != null ? key.GetValueKind(t.Name) : RegistryValueKind.DWord;
                _previous.Add(new Snapshot(t.Hive, t.Path, t.Name, existing, kind));
                key.SetValue(t.Name, 0, RegistryValueKind.DWord);
                key.Dispose();
            }
            // Auto Game Mode'u ise tam tersine açık tutalım (Windows oyun moduna yardımcı)
            var gmKey = OpenKey("HKCU", @"Software\Microsoft\GameBar", true);
            if (gmKey != null)
            {
                gmKey.SetValue("AutoGameModeEnabled", 1, RegistryValueKind.DWord);
                gmKey.Dispose();
            }
            return true;
        }
        catch { return false; }
    }

    public void Deactivate()
    {
        foreach (var s in _previous)
        {
            try
            {
                var key = OpenKey(s.Hive, s.Path, true);
                if (key == null) continue;
                if (s.Value == null) key.DeleteValue(s.Name, false);
                else key.SetValue(s.Name, s.Value, s.Kind);
                key.Dispose();
            }
            catch { }
        }
        _previous.Clear();
    }

    private static RegistryKey? OpenKey(string hive, string path, bool writable)
    {
        var root = hive switch
        {
            "HKCU" => Registry.CurrentUser,
            "HKLM" => Registry.LocalMachine,
            _ => null
        };
        return root?.CreateSubKey(path, writable);
    }
}
