using System.Windows;

namespace TurboMode.Views;

public partial class CloseDialog : Window
{
    public enum Action { Cancel, MinimizeToTray, FullExit }

    public Action ChosenAction { get; private set; } = Action.Cancel;
    public bool RememberChoice => DontAskAgain.IsChecked == true;

    public CloseDialog()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        ChosenAction = Action.Cancel;
        DialogResult = false;
        Close();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        ChosenAction = Action.FullExit;
        DialogResult = true;
        Close();
    }

    private void Tray_Click(object sender, RoutedEventArgs e)
    {
        ChosenAction = Action.MinimizeToTray;
        DialogResult = true;
        Close();
    }
}
