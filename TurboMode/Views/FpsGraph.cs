using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TurboMode.Services;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace TurboMode.Views;

/// <summary>
/// FPS / frametime grafiği — son 60 saniye, 1 sn aralıklarla güncellenir.
/// FpsMonitor'dan canlı veri çeker, Polyline ile çizer.
/// </summary>
public sealed class FpsGraph : Canvas
{
    private readonly Polyline _line = new()
    {
        Stroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x29)),
        StrokeThickness = 1.5,
        StrokeLineJoin = PenLineJoin.Round,
    };
    private readonly TextBlock _maxLabel = new() { FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(0x70, 0x70, 0x80)) };
    private readonly TextBlock _minLabel = new() { FontSize = 9, Foreground = new SolidColorBrush(Color.FromRgb(0x70, 0x70, 0x80)) };
    private readonly DispatcherTimer _timer;
    private FpsMonitor? _monitor;

    // Son 60 sn'lik FPS değerleri (1 sn aralıkla sample)
    private readonly Queue<double> _samples = new();
    private const int SampleCount = 60;

    public FpsGraph()
    {
        Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
        ClipToBounds = true;
        Height = 60;
        Children.Add(_line);
        Children.Add(_maxLabel);
        Children.Add(_minLabel);
        SetTop(_maxLabel, 2); SetLeft(_maxLabel, 4);
        SetBottom(_minLabel, 2); SetLeft(_minLabel, 4);

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => Refresh();

        SizeChanged += (_, _) => Render();
    }

    public void Bind(FpsMonitor monitor)
    {
        _monitor = monitor;
        _samples.Clear();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _samples.Clear();
        _line.Points.Clear();
    }

    private void Refresh()
    {
        if (_monitor == null) return;
        var fps = _monitor.AverageFps(2); // son 2 sn ortalama
        _samples.Enqueue(fps);
        while (_samples.Count > SampleCount) _samples.Dequeue();
        Render();
    }

    private void Render()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0) return;
        _line.Points.Clear();
        if (_samples.Count < 2) return;

        var vals = _samples.ToArray();
        var max = vals.Where(v => v > 0).DefaultIfEmpty(0).Max();
        var min = vals.Where(v => v > 0).DefaultIfEmpty(0).Min();
        if (max == 0) return;

        // Y ekseni: min 0.9, max 1.1 ile padding
        var yMin = Math.Max(0, min * 0.85);
        var yMax = max * 1.05;
        var range = Math.Max(1, yMax - yMin);

        var stepX = ActualWidth / Math.Max(1, SampleCount - 1);
        for (int i = 0; i < vals.Length; i++)
        {
            var v = vals[i];
            if (v <= 0) continue;
            var x = (vals.Length - 1 - i) * stepX;
            // Son sample (i = length-1) en sağda olmalı, en eski en solda
            x = i * stepX;
            var y = ActualHeight - ((v - yMin) / range) * ActualHeight;
            _line.Points.Add(new Point(x, y));
        }

        _maxLabel.Text = $"max {max:0} FPS";
        _minLabel.Text = $"min {min:0} FPS";
    }
}
