namespace TurboMode.Models;

public sealed class SessionRecord
{
    public DateTime StartedUtc { get; set; }
    public DateTime EndedUtc { get; set; }
    public string GameName { get; set; } = "";
    public string GameProcess { get; set; } = "";
    public int SuspendedProcesses { get; set; }
    public int StoppedServices { get; set; }
    public long RamFreedMb { get; set; }
    public double BaselineFps { get; set; }
    public double ActiveFps { get; set; }

    public double FpsGainPercent =>
        BaselineFps > 5 ? (ActiveFps - BaselineFps) / BaselineFps * 100.0 : 0;

    public TimeSpan Duration => EndedUtc - StartedUtc;

    public string DurationLabel => Duration.TotalHours >= 1
        ? $"{(int)Duration.TotalHours} sa {Duration.Minutes} dk"
        : $"{Duration.Minutes} dk";
}
