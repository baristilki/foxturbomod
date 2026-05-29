namespace TurboMode.Config;

/// <summary>
/// Hangi süreç/servislere dokunulup hangilerine dokunulmayacağını burada belirliyoruz.
/// Beyaz liste (Whitelist) <b>asla</b> askıya alınmaz; oyun ile birlikte çalışması zorunludur.
/// </summary>
public static class OptimizationTargets
{
    /// <summary>
    /// Askıya alınacak, sıkça oyun sırasında CPU/RAM tüketen masaüstü uygulamaları.
    /// Process adı (.exe olmadan), case-insensitive.
    /// </summary>
    public static readonly string[] ProcessTargets =
    {
        // Bulut senkronizasyonu
        "OneDrive",
        "Dropbox",
        "GoogleDriveFS",
        "iCloudServices",
        // Müzik / yardımcılar
        "Spotify",
        "SpotifyWebHelper",
        // Tarayıcı arka planı (kullanıcı kapatmadıysa askıya al — bellek çok şişer)
        "chrome",
        "msedge",
        "firefox",
        "opera",
        "brave",
        // Adobe arka plan servisleri
        "Adobe Desktop Service",
        "AdobeIPCBroker",
        "CCXProcess",
        "CCLibrary",
        "Creative Cloud",
        // Office/ofis arka planı
        "OfficeClickToRun",
        "MSOSYNC",
        // Mesajlaşma (oyun dışı) — Slack/Teams. Discord BURADA DEĞİL.
        "Teams",
        "ms-teams",
        "Slack",
        "WhatsApp",
        "Telegram",
        // Launcher'lar (oyun zaten açıldıktan sonra arka planda lazım değil)
        "EpicGamesLauncher",
        "EpicWebHelper",
        "Battle.net",
        "Agent", // Battle.net Update Agent
        "GalaxyClient",
        "GOG Galaxy Notifications Renderer",
        // Razer/Logitech/Corsair vb. ağır overlayler — varsa
        "RzSDKService",
        "RazerCentralService",
        "LCore",
        "iCUE",
        // OEM uygulamaları
        "ArmouryCrate.Service",
        "AsusOptimization",
        "DellSupportAssist",
    };

    /// <summary>
    /// Beyaz liste — bu süreçler ne olursa olsun ASLA durdurulmaz/askıya alınmaz.
    /// Oyuncunun oyun esnasında ihtiyaç duyacağı uygulamalar burada.
    /// </summary>
    public static readonly string[] ProcessWhitelist =
    {
        // Sesli sohbet — oyuncu Discord ile konuşuyor, dokunma
        "Discord",
        "DiscordCanary",
        "DiscordPTB",
        "Discord Helper",
        "TeamSpeak3",
        "ts3client_win64",
        "Mumble",
        "vrserver",     // SteamVR
        // OBS / streaming
        "obs64",
        "obs32",
        "obs-browser-page",
        "StreamlabsOBS",
        // Sistem ve kabuk — kesinlikle dokunma
        "explorer",
        "dwm",
        "csrss",
        "winlogon",
        "lsass",
        "services",
        "svchost",
        "System",
        "Idle",
        "Registry",
        "smss",
        "wininit",
        // Anti-virüs — durdurulması güvenlik riski
        "MsMpEng",
        "NisSrv",
        "SecurityHealthService",
        "SecurityHealthSystray",
        "avp",
        "avgnt",
        "bdagent",
        // Oyun launcher'larını oyun açıldıysa askıya almak işe yaramaz; kendileri filtrelenecek
        "steam",
        "RiotClientServices",
        "RiotClientUx",
        "VALORANT",
        // Bizim uygulama
        "FoxMod",
        "TurboMode",
    };

    /// <summary>
    /// Durdurulacak Windows servisleri. Hepsi güvenli — geri alındığında sorun çıkarmaz.
    /// </summary>
    public static readonly ServiceTarget[] ServiceTargets =
    {
        new("WSearch",       "Windows Search (indeksleme)"),
        new("SysMain",       "SysMain / Superfetch"),
        new("DiagTrack",     "Bağlantılı Kullanıcı Deneyimleri ve Telemetri"),
        new("dmwappushservice","WAP Push Mesaj Yönlendirme"),
        new("MapsBroker",    "İndirilen Haritalar Yöneticisi"),
        new("WerSvc",        "Windows Hata Raporlama"),
        new("Fax",           "Faks"),
        new("RetailDemo",    "Mağaza Demo Servisi"),
        new("PrintNotify",   "Yazıcı Genişletmeleri"),
    };

    /// <summary>
    /// Geçici durdurulması güvenli olmayan, ASLA dokunulmayacak servisler.
    /// (ServiceTargets dışı zaten dokunulmuyor; bu liste savunma amaçlı çift kontroldür.)
    /// </summary>
    public static readonly string[] ServiceBlacklist =
    {
        "RpcSs", "DcomLaunch", "RpcEptMapper", "BFE", "MpsSvc",
        "AudioSrv", "Audiosrv", "AudioEndpointBuilder",
        "Themes", "ProfSvc", "TermService", "UserManager",
        "wuauserv", // Yorum: Windows Update durdurmak güvenli ama bazı senaryolarda istenmiyor; başlangıçta dışarıda.
    };

    public record ServiceTarget(string ServiceName, string DisplayName);
}
