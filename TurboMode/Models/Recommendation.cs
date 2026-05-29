namespace TurboMode.Models;

public enum RecommendationSeverity { Info, Warning, Critical, Good }

public sealed class Recommendation
{
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string? ActionLabel { get; init; }       // "Kapatma Rehberi" gibi
    public string? ActionUrl { get; init; }         // tıklayınca açılacak link
    public string? ActionCommand { get; init; }     // "clear-shader-cache" gibi iç-komut
    public string IconEmoji { get; init; } = "ℹ";
    public RecommendationSeverity Severity { get; init; } = RecommendationSeverity.Info;
    public string FpsImpact { get; init; } = "";    // "%5-15 FPS kaybı" gibi
}
