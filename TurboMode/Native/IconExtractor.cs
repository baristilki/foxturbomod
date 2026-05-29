using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TurboMode.Native;

/// <summary>
/// Bir .exe dosyasının ikonunu çıkarır. Başarısızsa varsayılan oyun ikonunu döner.
/// </summary>
internal static class IconExtractor
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private static readonly Lazy<BitmapSource?> _defaultIcon = new(() =>
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/default_game.png", UriKind.Absolute);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = uri;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    });

    public static BitmapSource? Extract(string exePath)
    {
        var fromExe = TryFromExe(exePath);
        return fromExe ?? _defaultIcon.Value;
    }

    private static BitmapSource? TryFromExe(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath)) return null;
        var info = new SHFILEINFO();
        uint flags = SHGFI_ICON | SHGFI_LARGEICON;
        if (!File.Exists(exePath)) flags |= SHGFI_USEFILEATTRIBUTES;

        var res = SHGetFileInfo(exePath, 0, ref info, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
        if (res == IntPtr.Zero || info.hIcon == IntPtr.Zero) return null;

        try
        {
            var bmp = Imaging.CreateBitmapSourceFromHIcon(
                info.hIcon, Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(48, 48));

            // Eğer ikon tamamen şeffafsa veya çok küçükse (boş ikon), null dön
            if (IsEmptyIcon(bmp))
            {
                DestroyIcon(info.hIcon);
                return null;
            }
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
        finally { DestroyIcon(info.hIcon); }
    }

    private static bool IsEmptyIcon(BitmapSource bmp)
    {
        try
        {
            // Çok basit kontrol: ikon piksel verisi yoksa
            return bmp.PixelWidth < 8 || bmp.PixelHeight < 8;
        }
        catch { return false; }
    }
}
