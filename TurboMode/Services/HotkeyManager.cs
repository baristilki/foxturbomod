using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TurboMode.Services;

/// <summary>
/// Global system-wide hotkey desteği (Ctrl+Alt+T gibi).
/// Win32 RegisterHotKey API kullanır. Window handle gerekir.
/// </summary>
public sealed class HotkeyManager : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [Flags]
    public enum Modifiers : uint
    {
        None = 0, Alt = 1, Control = 2, Shift = 4, Win = 8
    }

    private const int WM_HOTKEY = 0x0312;
    private HwndSource? _source;
    private readonly Dictionary<int, Action> _handlers = new();
    private int _nextId = 9000;

    public event Action<int>? HotkeyPressed;

    public void Attach(Window window)
    {
        var helper = new WindowInteropHelper(window);
        if (helper.Handle == IntPtr.Zero)
        {
            window.SourceInitialized += (_, _) => Attach(window);
            return;
        }
        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(WndProc);
    }

    public int Register(Modifiers mod, uint key, Action handler)
    {
        if (_source == null) throw new InvalidOperationException("HotkeyManager.Attach() önce çağrılmalı");
        var id = _nextId++;
        if (!RegisterHotKey(_source.Handle, id, (uint)mod, key))
        {
            Log.Warn("Hotkey kaydedilemedi: mod={0} key={1}", mod, key);
            return -1;
        }
        _handlers[id] = handler;
        return id;
    }

    public void Unregister(int id)
    {
        if (_source != null) UnregisterHotKey(_source.Handle, id);
        _handlers.Remove(id);
    }

    public void UnregisterAll()
    {
        foreach (var id in _handlers.Keys.ToList()) Unregister(id);
    }

    public static (Modifiers mod, uint key) Parse(string combo)
    {
        if (string.IsNullOrWhiteSpace(combo)) return (Modifiers.None, 0);
        var parts = combo.Split('+').Select(p => p.Trim()).ToArray();
        Modifiers mod = Modifiers.None;
        uint key = 0;
        foreach (var p in parts)
        {
            if (p.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                p.Equals("Control", StringComparison.OrdinalIgnoreCase)) mod |= Modifiers.Control;
            else if (p.Equals("Alt", StringComparison.OrdinalIgnoreCase)) mod |= Modifiers.Alt;
            else if (p.Equals("Shift", StringComparison.OrdinalIgnoreCase)) mod |= Modifiers.Shift;
            else if (p.Equals("Win", StringComparison.OrdinalIgnoreCase) ||
                     p.Equals("Windows", StringComparison.OrdinalIgnoreCase)) mod |= Modifiers.Win;
            else if (p.Length == 1)
            {
                var c = char.ToUpper(p[0]);
                key = (uint)c;
            }
            else if (p.StartsWith("F", StringComparison.OrdinalIgnoreCase) &&
                     int.TryParse(p.AsSpan(1), out var fn) && fn >= 1 && fn <= 24)
            {
                key = (uint)(0x70 + fn - 1);
            }
        }
        return (mod, key);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            if (_handlers.TryGetValue(id, out var h))
            {
                handled = true;
                h.Invoke();
                HotkeyPressed?.Invoke(id);
            }
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_source != null)
        {
            foreach (var id in _handlers.Keys.ToList()) Unregister(id);
            _source.RemoveHook(WndProc);
            _source = null;
        }
    }
}
