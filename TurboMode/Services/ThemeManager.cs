using System.Windows;
using Application = System.Windows.Application;

namespace TurboMode.Services;

public enum AppTheme { Fox, Cyber, Razer }

public static class ThemeManager
{
    public static AppTheme Current { get; private set; } = AppTheme.Fox;
    public static event Action<AppTheme>? ThemeChanged;

    public static void Apply(AppTheme theme)
    {
        try
        {
            var name = theme switch
            {
                AppTheme.Cyber => "CyberTheme",
                AppTheme.Razer => "RazerTheme",
                _ => "FoxTheme",
            };
            var uri = new Uri($"/Themes/{name}.xaml", UriKind.Relative);
            var dict = new ResourceDictionary { Source = uri };

            // Mevcut tema dictionary'lerini çıkar
            var merged = Application.Current.Resources.MergedDictionaries;
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                var src = merged[i].Source?.OriginalString ?? "";
                if (src.Contains("Theme.xaml")) merged.RemoveAt(i);
            }

            // Yeni temayı EN SONA ekle — WPF MergedDictionaries lookup
            // önceliği son eklenene aittir (DynamicResource bunu kullanır)
            merged.Add(dict);
            Current = theme;
            ThemeChanged?.Invoke(theme);
            Log.Info($"Tema değiştirildi: {theme}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Tema uygulanamadı: {Theme}", theme);
        }
    }

    public static AppTheme Parse(string? name) => name switch
    {
        "Cyber" => AppTheme.Cyber,
        "Razer" => AppTheme.Razer,
        _ => AppTheme.Fox,
    };
}
