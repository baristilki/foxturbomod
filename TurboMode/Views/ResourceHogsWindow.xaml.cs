using System.Windows;
using TurboMode.Services;

namespace TurboMode.Views;

public partial class ResourceHogsWindow : Window
{
    public ResourceHogsWindow(ResourceHogsMonitor monitor)
    {
        InitializeComponent();
        HogsList.ItemsSource = monitor.Items;
    }
}
