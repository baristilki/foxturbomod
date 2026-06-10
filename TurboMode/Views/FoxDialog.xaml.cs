using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;

namespace TurboMode.Views;

public partial class FoxDialog : Window
{
    public enum DialogKind { Info, Warning, Confirm, Error, Success }

    public string? Result { get; private set; }

    public FoxDialog()
    {
        InitializeComponent();
    }

    public static string? Show(Window? owner, string title, string message,
        DialogKind kind = DialogKind.Info, params string[] buttons)
    {
        var dlg = new FoxDialog
        {
            Owner = owner ?? System.Windows.Application.Current?.MainWindow,
        };
        dlg.TitleLabel.Text = title;
        dlg.MessageLabel.Text = message;
        dlg.IconLabel.Text = kind switch
        {
            DialogKind.Warning => "⚠",
            DialogKind.Confirm => "❓",
            DialogKind.Error => "❌",
            DialogKind.Success => "✓",
            _ => "ℹ",
        };

        if (buttons.Length == 0) buttons = new[] { "Tamam" };

        for (int i = 0; i < buttons.Length; i++)
        {
            var label = buttons[i];
            var btn = new Button
            {
                Content = label,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(i == 0 ? 0 : 8, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                MinWidth = 90,
            };
            // İlk buton accent (primary action)
            if (i == buttons.Length - 1)
            {
                btn.Background = (System.Windows.Media.Brush)dlg.FindResource("AccentBrush");
                btn.Foreground = System.Windows.Media.Brushes.White;
            }
            btn.Click += (_, _) => { dlg.Result = label; dlg.DialogResult = true; dlg.Close(); };
            dlg.ButtonPanel.Children.Add(btn);
        }

        dlg.ShowDialog();
        return dlg.Result;
    }

    public static bool Confirm(Window? owner, string title, string message) =>
        Show(owner, title, message, DialogKind.Confirm, "İptal", "Devam Et") == "Devam Et";

    public static void Info(Window? owner, string title, string message) =>
        Show(owner, title, message, DialogKind.Info, "Tamam");

    public static void Warn(Window? owner, string title, string message) =>
        Show(owner, title, message, DialogKind.Warning, "Tamam");

    public static void Error(Window? owner, string title, string message) =>
        Show(owner, title, message, DialogKind.Error, "Tamam");

    public static void Success(Window? owner, string title, string message) =>
        Show(owner, title, message, DialogKind.Success, "Tamam");
}
