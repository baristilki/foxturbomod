using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TurboMode.Native;

/// <summary>
/// TextBlock için sayı sayma animasyonu attached property.
/// Kullanım: <TextBlock local:AnimatedNumber.Value="{Binding TargetValue}" local:AnimatedNumber.Format="0 FPS" />
/// </summary>
public static class AnimatedNumber
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
        "Value", typeof(double), typeof(AnimatedNumber),
        new PropertyMetadata(0.0, OnValueChanged));

    public static double GetValue(DependencyObject obj) => (double)obj.GetValue(ValueProperty);
    public static void SetValue(DependencyObject obj, double v) => obj.SetValue(ValueProperty, v);

    public static readonly DependencyProperty FormatProperty = DependencyProperty.RegisterAttached(
        "Format", typeof(string), typeof(AnimatedNumber),
        new PropertyMetadata("0"));

    public static string GetFormat(DependencyObject obj) => (string)obj.GetValue(FormatProperty);
    public static void SetFormat(DependencyObject obj, string v) => obj.SetValue(FormatProperty, v);

    private static readonly DependencyProperty CurrentDisplayValueProperty = DependencyProperty.RegisterAttached(
        "CurrentDisplayValue", typeof(double), typeof(AnimatedNumber),
        new PropertyMetadata(0.0, OnDisplayChanged));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock tb) return;
        var to = (double)e.NewValue;
        var from = (double)tb.GetValue(CurrentDisplayValueProperty);

        var anim = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(700),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        tb.BeginAnimation(CurrentDisplayValueProperty, anim);
    }

    private static void OnDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock tb) return;
        var v = (double)e.NewValue;
        var fmt = GetFormat(tb);
        if (string.IsNullOrEmpty(fmt)) fmt = "0";
        try { tb.Text = v.ToString(fmt); } catch { tb.Text = v.ToString("0"); }
    }
}
