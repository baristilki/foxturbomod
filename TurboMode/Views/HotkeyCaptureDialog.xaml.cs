using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace TurboMode.Views;

public partial class HotkeyCaptureDialog : Window
{
    public string? CapturedCombo { get; private set; }

    public HotkeyCaptureDialog(string title, string currentCombo)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        ComboLabel.Text = currentCombo ?? "—";
        CapturedCombo = currentCombo;
        Loaded += (_, _) => Focus();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        // Modifier-only basışlar combo değil
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin) return;

        var parts = new System.Collections.Generic.List<string>();
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");

        // F-key veya alfa-num
        string keyName = key.ToString();
        parts.Add(keyName);

        CapturedCombo = string.Join("+", parts);
        ComboLabel.Text = CapturedCombo;
        e.Handled = true;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        CapturedCombo = null;
        DialogResult = false;
        Close();
    }
}
