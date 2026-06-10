using Microsoft.Win32;

namespace TurboMode.Services;

/// <summary>
/// Windows başlangıcında Fox Turbo Mod otomatik başlatma.
/// HKCU\Software\Microsoft\Windows\CurrentVersion\Run kayıt defteri.
/// </summary>
public static class AutoStartManager
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "FoxTurboMod";

    public static bool IsEnabled
    {
        get
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(RunKey);
                var v = k?.GetValue(ValueName) as string;
                return !string.IsNullOrEmpty(v);
            }
            catch { return false; }
        }
    }

    public static bool SetEnabled(bool enabled)
    {
        try
        {
            using var k = Registry.CurrentUser.CreateSubKey(RunKey, true);
            if (k == null) return false;
            if (enabled)
            {
                var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exe)) return false;
                k.SetValue(ValueName, $"\"{exe}\"");
                Log.Info("AutoStart enabled: {0}", exe);
            }
            else
            {
                try { k.DeleteValue(ValueName, false); } catch { }
                Log.Info("AutoStart disabled");
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "AutoStart toggle hatası");
            return false;
        }
    }
}
