using System.Windows.Media.Imaging;

namespace TurboMode.Models;

public sealed class InstalledGame
{
    public required string DisplayName { get; init; }
    public required string ExecutablePath { get; init; }
    public required string ProcessName { get; init; }    // "VALORANT-Win64-Shipping" gibi (.exe olmadan)
    public string? Platform { get; init; }               // "Steam", "Epic", "Riot", "Diğer"
    public BitmapSource? Icon { get; set; }              // 64x64 ikon
    public bool IsRunning { get; set; }
}
