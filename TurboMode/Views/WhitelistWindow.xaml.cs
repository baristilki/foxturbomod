using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using TurboMode.Config;
using TurboMode.Models;
using TurboMode.Services;

namespace TurboMode.Views;

public partial class WhitelistWindow : Window
{
    private readonly AppSettings _settings;
    private readonly List<ProcessItem> _items = new();

    public WhitelistWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadProcessList();
        ProcessList.ItemsSource = _items;
    }

    private void LoadProcessList()
    {
        var defaultWhite = new HashSet<string>(OptimizationTargets.ProcessWhitelist,
            StringComparer.OrdinalIgnoreCase);
        var customWhite = new HashSet<string>(_settings.CustomWhitelist,
            StringComparer.OrdinalIgnoreCase);

        var groups = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.ProcessName))
            .GroupBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new
            {
                Name = g.Key,
                TotalRam = g.Sum(p => { try { return p.WorkingSet64; } catch { return 0L; } }),
            })
            .OrderByDescending(x => x.TotalRam)
            .Take(80);

        foreach (var g in groups)
        {
            _items.Add(new ProcessItem
            {
                ProcessName = g.Name,
                RamLabel = $"{g.TotalRam / 1024 / 1024} MB" +
                           (defaultWhite.Contains(g.Name) ? "  (varsayılan korunan)" : ""),
                IsWhitelisted = defaultWhite.Contains(g.Name) || customWhite.Contains(g.Name),
                IsDefault = defaultWhite.Contains(g.Name),
            });
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Sadece kullanıcının manuel eklediği ve varsayılan olmayan adları kaydet
        _settings.CustomWhitelist = _items
            .Where(i => i.IsWhitelisted && !i.IsDefault)
            .Select(i => i.ProcessName)
            .ToList();
        ProfileStore.Save(_settings);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public class ProcessItem : INotifyPropertyChanged
    {
        private bool _isWhitelisted;
        public string ProcessName { get; set; } = "";
        public string RamLabel { get; set; } = "";
        public bool IsDefault { get; set; }
        public bool IsWhitelisted
        {
            get => _isWhitelisted;
            set { _isWhitelisted = value; PropertyChanged?.Invoke(this, new(nameof(IsWhitelisted))); }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
