using System.Diagnostics;
using System.Management;

namespace TurboMode.Services;

public static class DriverChecker
{
    public sealed record DriverInfo(
        string Vendor, string Name, string Version, DateTime? Date,
        string? DownloadUrl, string Category, string Health);

    /// <summary>Sadece GPU + ses + ağ + chipset sürücülerini tara.</summary>
    public static List<DriverInfo> Scan()
    {
        var result = new List<DriverInfo>();
        result.AddRange(ScanGpu());
        result.AddRange(ScanByClass("MEDIA", "Ses", null));
        result.AddRange(ScanByClass("Net", "Ağ", null));
        result.AddRange(ScanByClass("System", "Chipset",
            n => n.Contains("Chipset", StringComparison.OrdinalIgnoreCase) ||
                 n.Contains("Platform Controller", StringComparison.OrdinalIgnoreCase)));
        return result;
    }

    public static List<DriverInfo> Check()
    {
        var result = new List<DriverInfo>();

        // NVIDIA — nvidia-smi
        try
        {
            var psi = new ProcessStartInfo("nvidia-smi.exe",
                "--query-gpu=driver_version,name --format=csv,noheader")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            };
            using var p = Process.Start(psi);
            if (p != null)
            {
                var line = p.StandardOutput.ReadLine();
                p.WaitForExit(2000);
                if (!string.IsNullOrEmpty(line))
                {
                    var parts = line.Split(',').Select(s => s.Trim()).ToArray();
                    if (parts.Length >= 2)
                        result.Add(new DriverInfo("NVIDIA", parts[1], parts[0], null,
                            "https://www.nvidia.com/Download/index.aspx", "GPU", "OK"));
                }
            }
        }
        catch { }

        // AMD + Intel — WMI'dan
        try
        {
            using var s = new ManagementObjectSearcher(
                "SELECT Name, DriverVersion, AdapterCompatibility FROM Win32_VideoController");
            foreach (var o in s.Get())
            {
                var name = o["Name"]?.ToString() ?? "";
                var version = o["DriverVersion"]?.ToString() ?? "?";
                var vendor = o["AdapterCompatibility"]?.ToString() ?? "";
                if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)) continue;

                string? url = null;
                string v = vendor;
                if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
                { v = "AMD"; url = "https://www.amd.com/en/support"; }
                else if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase))
                { v = "Intel"; url = "https://www.intel.com/content/www/us/en/download-center/home.html"; }

                result.Add(new DriverInfo(v, name, version, null, url, "GPU", "OK"));
            }
        }
        catch { }

        return result;
    }

    private static List<DriverInfo> ScanGpu() => Check();

    private static List<DriverInfo> ScanByClass(string deviceClass, string category, Func<string, bool>? nameFilter)
    {
        var list = new List<DriverInfo>();
        try
        {
            using var s = new ManagementObjectSearcher(
                $"SELECT DeviceName, DriverVersion, DriverDate, Manufacturer FROM Win32_PnPSignedDriver WHERE DeviceClass='{deviceClass}'");
            foreach (var o in s.Get())
            {
                var name = o["DeviceName"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name)) continue;
                if (nameFilter != null && !nameFilter(name)) continue;
                var version = o["DriverVersion"]?.ToString() ?? "?";
                var manufacturer = o["Manufacturer"]?.ToString() ?? "";
                var dateRaw = o["DriverDate"]?.ToString();
                DateTime? date = null;
                if (!string.IsNullOrEmpty(dateRaw) && dateRaw.Length >= 8)
                {
                    if (DateTime.TryParseExact(dateRaw.Substring(0, 8),
                        "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var d))
                        date = d;
                }

                // Sağlık: 2 yıldan eskiyse uyarı
                var health = "OK";
                if (date.HasValue && (DateTime.Now - date.Value).TotalDays > 730) health = "Eski (2+ yıl)";
                else if (date.HasValue && (DateTime.Now - date.Value).TotalDays > 365) health = "Eski (1+ yıl)";

                list.Add(new DriverInfo(manufacturer, name, version, date, null, category, health));
            }
        }
        catch (Exception ex) { Log.Error(ex, "ScanByClass hatası: {0}", deviceClass); }
        return list;
    }
}
