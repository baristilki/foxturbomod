using TurboMode.Models;

namespace TurboMode.Services;

public static class RecommendationsServiceHelpers
{
    public static string LastNetworkSummary = "ölçülüyor...";
    public static string LastDriverSummary = "okunuyor...";
}

public static class RecommendationsService
{
    public static List<Recommendation> Build(SystemStateChecker.SystemState state)
    {
        var list = new List<Recommendation>();
        AddDriverScanCard(list);

        // VBS / HVCI — direkt kapat butonu
        if (state.VbsEnabled || state.HvciEnabled)
        {
            list.Add(new Recommendation
            {
                IconEmoji = "🛑",
                Severity = RecommendationSeverity.Critical,
                Title = "VBS / Bellek Bütünlüğü AÇIK",
                Description =
                    "Windows 11'in CPU sanallaştırma tabanlı güvenliği. Ryzen sistemlerde %5-15 FPS yer. " +
                    "Kapatmak güvenlik özelliklerini devre dışı bırakır — kötü amaçlı driver koruması düşer. " +
                    "Rebootla aktif olur.",
                FpsImpact = "%5-15 FPS",
                ActionLabel = "VBS'yi Kapat",
                ActionCommand = "disable-vbs"
            });
        }
        else
        {
            list.Add(new Recommendation
            {
                IconEmoji = "✅",
                Severity = RecommendationSeverity.Good,
                Title = "VBS kapalı",
                Description = "Bellek Bütünlüğü kapalı — oyun için optimum.",
            });
        }

        // Hypervisor — direkt kapat butonu
        if (state.HypervisorPresent)
        {
            list.Add(new Recommendation
            {
                IconEmoji = "⚠",
                Severity = RecommendationSeverity.Warning,
                Title = "Hipervizor AKTİF",
                Description =
                    "Hyper-V / WSL2 / Docker / Sandbox / VBS bir hipervizor çalıştırıyor. " +
                    "Bunlardan birine ihtiyacın yoksa kapatmak %3-8 FPS kazanç. Rebootla aktif olur.",
                FpsImpact = "%3-8 FPS",
                ActionLabel = "Hipervizoru Kapat",
                ActionCommand = "disable-hypervisor"
            });
        }

        // HAGS — link açma (toggle reboot ister, manuel)
        if (state.HagsSupported && !state.HagsEnabled)
        {
            list.Add(new Recommendation
            {
                IconEmoji = "💡",
                Severity = RecommendationSeverity.Warning,
                Title = "HAGS kapalı",
                Description = "Hardware-Accelerated GPU Scheduling açık olmalı.",
                FpsImpact = "%2-5 FPS",
                ActionLabel = "Grafik Ayarlarını Aç",
                ActionUrl = "ms-settings:display-advancedgraphics"
            });
        }
        else if (state.HagsEnabled)
        {
            list.Add(new Recommendation
            {
                IconEmoji = "✅",
                Severity = RecommendationSeverity.Good,
                Title = "HAGS aktif",
                Description = "Hardware-Accelerated GPU Scheduling açık — optimum.",
            });
        }

        // Discord — direkt kapat butonu
        if (state.DiscordRunning)
        {
            list.Add(new Recommendation
            {
                IconEmoji = "💬",
                Severity = RecommendationSeverity.Warning,
                Title = "Discord çalışıyor",
                Description =
                    "Discord overlay her frame'i hook eder ve %3-7 FPS yer. " +
                    "Discord'u tamamen kapatmak overlay'i de durdurur. " +
                    "(Sesli görüşme yapıyorsan: Discord ayarlarından overlay'i manuel kapat.)",
                FpsImpact = "%3-7 FPS",
                ActionLabel = "Discord'u Kapat",
                ActionCommand = "close-discord"
            });
        }

        // NVIDIA Overlay — direkt kapat butonu
        if (state.GeforceOverlayRunning)
        {
            list.Add(new Recommendation
            {
                IconEmoji = "🎯",
                Severity = RecommendationSeverity.Warning,
                Title = "NVIDIA Overlay çalışıyor",
                Description =
                    "GeForce Experience / NVIDIA App overlay FPS düşürür. " +
                    "Overlay process'leri kapatılır — kalıcı için GeForce ayarlarından da kapat.",
                FpsImpact = "%1-3 FPS",
                ActionLabel = "NVIDIA Overlay'i Kapat",
                ActionCommand = "close-nvidia-overlay"
            });
        }

        // DNS — Discord/sosyal medya erişim sorunu
        list.Add(new Recommendation
        {
            IconEmoji = "🌐",
            Severity = RecommendationSeverity.Info,
            Title = "Discord açılmıyor? — Adım 1: DNS değiştir",
            Description =
                "Bazı ISP'ler DNS-tabanlı engelleme uygular. Cloudflare (1.1.1.1) DNS'e geçmek bunu çoğu durumda atlatır. " +
                "Bu yetmezse aşağıdaki DPI Bypass'i dene.",
            ActionLabel = "Cloudflare DNS + Discord Aç",
            ActionCommand = "dns-and-discord",
        });

        list.Add(new Recommendation
        {
            IconEmoji = "🛡",
            Severity = RecommendationSeverity.Info,
            Title = "Discord hala açılmıyor? — Adım 2: DPI Bypass (GoodbyeDPI)",
            Description =
                "ISP, TLS Client Hello paketlerine DPI (Deep Packet Inspection) uygularsa DNS yetmez. " +
                "GoodbyeDPI (açık kaynak, MIT) TLS paketlerini parçalayarak DPI'ı atlatır — Türkiye'de Discord için yaygın çözüm. " +
                "Discord açıldıktan sonra arka planda çalışmaya devam eder. " +
                "⚠ Valorant/Vanguard açmadan önce 'Durdur' demelisin.",
            ActionLabel = "GoodbyeDPI Başlat + Discord Aç",
            ActionCommand = "dpi-and-discord",
        });

        list.Add(new Recommendation
        {
            IconEmoji = "⏹",
            Severity = RecommendationSeverity.Info,
            Title = "GoodbyeDPI'i Durdur",
            Description =
                "DPI bypass'i kapat. Valorant/Vanguard açmadan önce mutlaka durdurmalısın — " +
                "WinDivert kernel driver Vanguard ile çakışabilir.",
            ActionLabel = "Durdur",
            ActionCommand = "dpi-stop",
        });

        list.Add(new Recommendation
        {
            IconEmoji = "↩",
            Severity = RecommendationSeverity.Info,
            Title = "DNS'i otomatik ayara döndür",
            Description = "DNS değişikliğini geri al, ISP'nin varsayılan DNS'ine dön.",
            ActionLabel = "DNS'i Geri Al",
            ActionCommand = "dns-restore",
        });

        // Shader Cache temizleme
        list.Add(new Recommendation
        {
            IconEmoji = "🧹",
            Severity = RecommendationSeverity.Info,
            Title = "DirectX / NVIDIA / AMD Shader Cache",
            Description =
                "Eski shader cache stutter yaratabilir. Tek tıkla temizle — risk yok, oyun yeniden derler.",
            ActionLabel = "Önbellekleri Temizle",
            ActionCommand = "clear-shader-cache",
        });

        // TEMP klasörleri temizleme
        list.Add(new Recommendation
        {
            IconEmoji = "🧹",
            Severity = RecommendationSeverity.Info,
            Title = "Windows TEMP klasörlerini temizle",
            Description =
                "%TEMP% ve C:\\Windows\\Temp klasörlerindeki kullanılmayan dosyaları siler. " +
                "Tipik kazanç 500MB-3GB. Kullanımdaki dosyalar atlanır — sıfır risk.",
            ActionLabel = "TEMP'i Temizle",
            ActionCommand = "clean-temp",
        });

        // Background Apps (Microsoft Store)
        list.Add(new Recommendation
        {
            IconEmoji = "📱",
            Severity = RecommendationSeverity.Info,
            Title = "Arka Plan Uygulamaları (Store)",
            Description =
                "Microsoft Store uygulamaları (Xbox, Mail, Posta, OneNote, Telefon Bağlantısı vb.) " +
                "arka planda yenilenir. Hepsini birden kapat → gizli CPU/RAM/ağ kazancı.",
            FpsImpact = "%1-3 FPS",
            ActionLabel = "Hepsini Kapat",
            ActionCommand = "disable-background-apps"
        });

        // Telemetry tasks
        list.Add(new Recommendation
        {
            IconEmoji = "📡",
            Severity = RecommendationSeverity.Info,
            Title = "Telemetri ve uyumluluk görevleri",
            Description =
                "Windows zamanlanmış görevleri (CompatTelRunner, MicrosoftCompatibilityAppraiser, " +
                "ProgramDataUpdater) CPU yiyebilir. Devre dışı bırak.",
            FpsImpact = "%1-2 FPS",
            ActionLabel = "Devre Dışı Bırak",
            ActionCommand = "disable-telemetry-tasks"
        });

        // GPU NVIDIA prefer max performance — link (NVIDIA Control Panel manuel)
        list.Add(new Recommendation
        {
            IconEmoji = "🎮",
            Severity = RecommendationSeverity.Info,
            Title = "NVIDIA: Maximum Performance modu",
            Description =
                "NVIDIA Control Panel → Manage 3D Settings → Power management mode → " +
                "'Prefer maximum performance'. Laptoplarda dikkat (pil yer).",
            FpsImpact = "%2-5 FPS",
            ActionLabel = "NVIDIA Control Panel'i Aç",
            ActionCommand = "open-nvcpl"
        });

        // MSI Mode — bilgi
        list.Add(new Recommendation
        {
            IconEmoji = "🔧",
            Severity = RecommendationSeverity.Info,
            Title = "MSI Mode (GPU IRQ)",
            Description = "GPU IRQ latency'sini 1-3 ms düşürür. MSI Utility v3 manuel araç.",
            FpsImpact = "%1-3 FPS",
            ActionLabel = "MSI Utility",
            ActionUrl = "https://forums.guru3d.com/threads/windows-line-based-vs-message-signaled-based-interrupts-msi-tool.378044/"
        });

        // Mouse + Reflex — bilgi
        list.Add(new Recommendation
        {
            IconEmoji = "🖱",
            Severity = RecommendationSeverity.Info,
            Title = "Mouse Polling Rate & NVIDIA Reflex",
            Description =
                "Mouse yazılımından (G Hub, Synapse) 1000 Hz polling kontrol et. " +
                "Destekleyen oyunlarda (Valorant, CS2, Fortnite, Apex, CoD) NVIDIA Reflex / AMD Anti-Lag'i aç.",
        });

        return list;
    }

    private static void AddDriverScanCard(List<Recommendation> list)
    {
        list.Add(new Recommendation
        {
            IconEmoji = "💿",
            Severity = RecommendationSeverity.Info,
            Title = "Sürücü Taraması",
            Description =
                "GPU, ses, ağ ve chipset sürücülerini tarar. 1+ yıl eski olanları sarı, 2+ yıl eski olanları kırmızı işaretler. " +
                "Sürücü güncelleme oyun stutter'larını ve crash'leri çözer.",
            ActionLabel = "Taramayı Aç",
            ActionCommand = "open-driver-scan",
        });
    }
}
