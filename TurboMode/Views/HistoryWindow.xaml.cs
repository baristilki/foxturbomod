using System.Windows;
using TurboMode.Services;

namespace TurboMode.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        InitializeComponent();
        try { Load(); }
        catch (Exception ex)
        {
            FoxDialog.Warn(this, "Fox Turbo Mod",
                "Geçmiş yüklenemedi:\n\n" + ex.Message);
        }
    }

    private void Load()
    {
        var recent = SessionHistoryStore.Recent(50);
        var (count, avgGain, totalRam, totalTime) = SessionHistoryStore.WeeklySummary();

        WeekSessionsLabel.Text = count.ToString();
        WeekAvgFpsLabel.Text = count > 0 ? $"+{avgGain:0.0}%" : "—";
        WeekTotalTimeLabel.Text = count > 0
            ? (totalTime.TotalHours >= 1
                ? $"{(int)totalTime.TotalHours}sa {totalTime.Minutes}dk"
                : $"{totalTime.Minutes}dk")
            : "—";
        WeekRamLabel.Text = count > 0 ? $"{totalRam} MB" : "—";

        if (recent.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            SessionsList.Visibility = Visibility.Collapsed;
        }
        else
        {
            SessionsList.ItemsSource = recent;
        }
    }
}
