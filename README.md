<div align="center">

<img src="TurboMode/Resources/FoxLogo.png" width="140" alt="Fox Turbo Mod"/>

# 🦊 Fox Turbo Mod

**Windows için açık kaynak, ücretsiz oyun performans yöneticisi.**

Arka plan süreçlerini askıya alır · Gerçek FPS ölçer · Discord erişim sorunlarını çözer · Tek tıkla sistem optimize eder

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue.svg)]()
[![Release](https://img.shields.io/github/v/release/baristilki/foxturbomod?color=orange)](https://github.com/baristilki/foxturbomod/releases)
[![Downloads](https://img.shields.io/github/downloads/baristilki/foxturbomod/total?color=brightgreen)](https://github.com/baristilki/foxturbomod/releases)
[![Stars](https://img.shields.io/github/stars/baristilki/foxturbomod?style=social)](https://github.com/baristilki/foxturbomod/stargazers)

### [📥 İndir](https://github.com/baristilki/foxturbomod/releases/latest) · [💬 Discord](https://discord.com/users/peacefox) · [🐛 Sorun Bildir](https://github.com/baristilki/foxturbomod/issues) · [⭐ Star Ver](https://github.com/baristilki/foxturbomod)

</div>

---

## 🎯 30 Saniyede Özet

Oyun açtığında otomatik devreye girer ve **10+ Windows optimizasyonunu** aynı anda uygular:

```
🧊 Arka plan süreçlerini askıya alır (kill etmez!)    🛑 Gereksiz Windows servislerini durdurur
🧠 Standby RAM cache'ini temizler (1-3 GB serbest)   ⚡ Yüksek Performans güç planı
🎬 Game DVR / Xbox Game Bar kapatır                  🚀 Oyun sürecine yüksek öncelik
🎨 Görsel efektleri minimize eder                    🔧 CPU core parking off
🎯 MMCSS gaming priority                             🖱 Mouse acceleration off
```

Oyun kapanınca **her şey otomatik geri alınır**. Cheat değildir, donanımı verimli kullandırır.

**Bonus**: PresentMon ile gerçek FPS ölçer, GoodbyeDPI ile Türkiye'deki Discord engellemesini atlatır, GitHub auto-update ile kendini günceller.

## 📊 Gerçek Etki

| PC Durumu | Beklenen FPS Kazancı |
|---|---|
| 8-16 GB RAM, dağınık masaüstü (Chrome 30+ sekme, OneDrive, Adobe) | **%15-30** |
| HDD'li sistem, Search Indexer aktif | %5-15 + stutter azalır |
| 32 GB RAM, SSD, az yazılım | %3-8 |
| GPU bottleneck oyun (RT açık Cyberpunk) | %0-2 |

> **Felsefe**: Var olan donanımı %100 verimle kullandırır. Yeni donanım almadan dağınık bir PC'yi optimum çalıştırır.

## ✨ Tüm Özellikler

### 🚀 Performans Optimizasyonları (otomatik, oyun açılınca)
- 🧊 **Süreç askıya alma** — 30+ tanımlı arka plan uygulamasını dondurur (OneDrive, Spotify, Chrome sekmeleri, Adobe, Slack, Teams...)
- 🛑 **Servis durdurma** — Windows Search, SysMain, DiagTrack, MapsBroker, W32Time + 5 güvenli servis
- 🧠 **Standby memory cache temizleme** — `NtSetSystemInformation` ile 1-3 GB ekstra RAM
- ⚡ **Güç planı yüksek performans** — `powercfg /setactive`
- 🎬 **Game DVR / Xbox Game Bar otomatik kapatma**
- 🚀 **Oyun sürecine `ProcessPriorityClass.High`**
- 🎨 **Görsel efekt minimize** — `VisualFXSetting=2`
- 🔧 **CPU core parking off** — Tüm çekirdekler aktif
- 🎯 **MMCSS gaming priority** — `SystemResponsiveness=0` + Games task
- 🖱 **Mouse acceleration off** — Gaming standart

### 📊 Gerçek Zamanlı Analiz
- 🎯 **PresentMon (Intel/Microsoft)** ile gerçek FPS ölçümü — ETW tabanlı, en doğru
- 📈 **Baseline ↔ Aktif FPS karşılaştırması** — "%17 kazandın" değil "87 FPS → 102 FPS (+17.2%)" net sayı
- 📊 **Seans geçmişi** — Her oyun seansı kaydedilir, haftalık özet ("12 oyun • +14% ort. FPS • 8.3 GB serbest")

### 🦊 Akıllı Oyun Tespiti
- **50+ oyun** otomatik tespit: Valorant, LoL, CS2, CoD, Delta Force, BF 2042, Apex, GTA V, EFT, Cyberpunk, Elden Ring, Helldivers 2, Marvel Rivals, Palworld...
- WMI `__InstanceCreationEvent` ile **uzun process adları** tam destekli (DeltaForceClient-Win64-Shipping.exe gibi)
- Steam / Epic / Riot kütüphanelerini otomatik tara, oyunları **ikonlarıyla** listele
- Listeden bir oyuna tıkla → **direkt başlat**

### 🚀 Sistem Önerileri Penceresi
Tek tıkla **direkt aksiyon** butonları:
- 🛑 **VBS / Bellek Bütünlüğü kapat** — %5-15 FPS (Ryzen)
- ⚠ **Hipervizor kapat** — %3-8 FPS (Hyper-V / WSL2 / Docker)
- 🌐 **DNS Cloudflare 1.1.1.1** + Discord aç (DNS-bazlı engelleme için)
- 🛡 **GoodbyeDPI DPI Bypass** + Discord aç (TLS SNI engelleme için — Türkiye)
- 💬 **Discord'u kapat** (overlay'i durdurmak için)
- 🎯 **NVIDIA Overlay'i kapat**
- 🧹 **Shader cache temizle** (D3D/NVIDIA/AMD)
- 📱 **Background Store uygulamalarını kapat**
- 📡 **Telemetri görevlerini devre dışı bırak**
- 🎮 **NVIDIA Control Panel direkt aç**

### 🛡 Güvenli Tasarım
- **Asla kill, sadece suspend** — Process kaybı yok, veri kaybı yok
- **Beyaz liste agresif**: Discord, OBS, antivirüs, sistem süreçleri **dokunulmaz**
- **PID snapshot**: Sadece *bizim* askıya aldıklarımızı resume ederiz
- **Servis snapshot+restore**: Zaten kapalıysa açmayız
- **SafetyNet**: Uygulama çökerse bir sonraki açılışta otomatik kurtarma
- **Beyaz liste düzenleyici** UI — Hangi süreçlere dokunulmasın özelleştir

### 🔄 Otomatik Güncelleme
- Açılışta GitHub Releases sorgulanır
- Yeni sürüm varsa logonun yanında "v1.X.X mevcut — Güncelle ↗" linki çıkar
- **Tek tıkla otomatik indir + kur + yeniden başlat** — manuel indirme yok

### 🎮 Kullanıcı Deneyimi
- 📍 **Tepsi ikonu + arka plan modu** — Pencere kapanınca tepsiye iner
- 🚪 **Akıllı kapatma diyaloğu** — "Tepside Tut" / "Tamamen Çık" + "Bir daha sorma"
- 🔒 **Tek instance kilidi** — Aynı anda 2 kez açılamaz
- ⏱ **Uptime göstergesi** — Uygulamanın ne kadar süredir açık olduğunu gösterir
- 🛡 **Sistem durumu paneli** — HAGS, DirectStorage, VBS, Hipervizor, Windows sürümü gerçek zamanlı

## 🎮 Anti-Cheat Uyumluluğu

| Oyun / Anti-Cheat | Durum |
|---|---|
| **CoD, Battlefield, CS2, Apex, PUBG, EFT, GTA V** | ✅ Sorun yok |
| **LoL, Dota 2, Overwatch 2, Delta Force (ACE)** | ✅ Sorun yok |
| **Valorant (Vanguard)** | ⚠ Vanguard çalışırken Fox Turbo Mod penceresini kapatmak önerilir (uygulama tepside arka planda kalmaya devam eder). PresentMon ETW'yi Vanguard "şüpheli telemetri" olarak işaretleyebilir. **Ban vermez.** GoodbyeDPI aktifse mutlaka önce kapat. |

## 📥 Kurulum

### En kolay yol (önerilen)
1. [**Releases**](https://github.com/baristilki/foxturbomod/releases/latest) sayfasından `FoxTurboMod.exe`'yi indir
2. Çift tıkla → Windows SmartScreen "**Daha fazla bilgi**" → "**Yine de çalıştır**"
3. UAC penceresinde **"Evet"** (servis yönetimi için yönetici yetkisi şart)
4. Bir oyun aç → Fox Turbo Mod otomatik devreye girer

### Kaynak koddan derleme
```bash
git clone https://github.com/baristilki/foxturbomod
cd foxturbomod
dotnet publish TurboMode/TurboMode.csproj -c Release -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true
```

Çıktı: `bin/Release/net8.0-windows/win-x64/publish/FoxTurboMod.exe`

## 🛠 Gereksinimler
- **Windows 10 / 11** (64-bit)
- **Yönetici yetkisi** (servis yönetimi için zorunlu)
- ~150 MB disk
- **.NET 8 runtime kurulu olmasına GEREK YOK** — gömülü (self-contained build)

## ❓ SSS (Sıkça Sorulan Sorular)

<details>
<summary><b>Bu cheat mi? Ban yer miyim?</b></summary>

Hayır. Fox Turbo Mod **donanım kimliğine, oyuna veya anti-cheat sürücülere dokunmaz**. Sadece Windows'un kullanıcı seviyesindeki yönetilebilir kaynaklarını oyun için yeniden organize eder — bu, Razer Cortex / Process Lasso / Game Mode'un yaptığıyla aynı şey. Anti-cheat'lerle uyumludur (Vanguard hariç — PresentMon yüzünden uyarı çıkabilir, ban yok).
</details>

<details>
<summary><b>PC'me kalıcı zarar verir mi?</b></summary>

Hayır. Her optimizasyon **snapshot + restore** mantığıyla çalışır. Oyun kapanınca tüm değişiklikler eski haline döner. Uygulama çökse bile SafetyNet sistemi bir sonraki açılışta donmuş süreçleri kurtarır.
</details>

<details>
<summary><b>Windows SmartScreen / Chrome neden uyarı veriyor?</b></summary>

Code signing sertifikamız henüz yok (~$200/yıl). Bu yüzden yeni + imzasız exe için tüm yeni yazılımlar gibi uyarı çıkar. "Daha fazla bilgi" → "Yine de çalıştır" deyince geçer. Açık kaynak olduğu için kodun her satırı incelenebilir.
</details>

<details>
<summary><b>Discord açılmıyor — Türkiye'de yasaklı?</b></summary>

**"🚀 Öneriler"** penceresinde 2 çözüm:
- **Adım 1**: Cloudflare DNS'e geç (DNS-bazlı engelleme için)
- **Adım 2**: GoodbyeDPI bypass (TLS SNI/DPI engelleme için — Türkiye için standart çözüm)

Her ikisi de yetmezse VPN gerek (uygulamamızın yapamayacağı).
</details>

<details>
<summary><b>FPS gerçekten %15-30 mu artıyor?</b></summary>

Bağlama göre değişir. 16 GB RAM'li dağınık masaüstünde (50+ Chrome sekmesi, OneDrive, Adobe arka planda) net %15-30 var. 64 GB RAM'li temiz makinede %0-3. PresentMon ile sen kendin gerçek sayıları görürsün — "Baseline 87 FPS → Aktif 102 FPS (+17.2%)" gibi.
</details>

<details>
<summary><b>Geçmiş veriler nerede saklanıyor?</b></summary>

`%LOCALAPPDATA%\FoxMod\` klasöründe:
- `settings.json` — kullanıcı ayarları + beyaz liste
- `history.json` — son 200 oyun seansı (FPS, RAM, süre)

Hiçbir veri internete gönderilmez. Tamamen yerel, offline çalışır.
</details>

<details>
<summary><b>Microsoft Family Safety beni engelliyor</b></summary>

Aile yöneticisi hesabından `account.microsoft.com/family` → uygulama için izin verilebilir. Ya da kalıcı çözüm: code signing sertifikası alınınca otomatik tanınır.
</details>

## 🏗 Mimari

<details>
<summary>Klasör yapısı (genişletmek için tıkla)</summary>

```
TurboMode/
├── Services/
│   ├── TurboCoordinator.cs              # Tüm optimizer'ları koordine eder
│   ├── ProcessOptimizer.cs              # NtSuspendProcess / NtResumeProcess
│   ├── ServiceOptimizer.cs              # Windows servisleri snapshot+restore
│   ├── MemoryOptimizer.cs               # NtSetSystemInformation cache temizle
│   ├── PowerPlanOptimizer.cs            # powercfg /setactive
│   ├── NetworkOptimizer.cs              # QoS policy + DNS flush
│   ├── GameDvrOptimizer.cs              # Game Bar registry tweaks
│   ├── ProcessPriorityOptimizer.cs      # Process priority class
│   ├── VisualEffectsOptimizer.cs        # VisualFXSetting registry
│   ├── CpuParkingOptimizer.cs           # powercfg CPMINCORES
│   ├── SystemResponsivenessOptimizer.cs # MMCSS gaming priority
│   ├── MouseOptimizer.cs                # Mouse acceleration off
│   ├── FpsMonitor.cs                    # PresentMon stdout parse
│   ├── GameDetector.cs                  # WMI __InstanceCreationEvent
│   ├── GameLibraryScanner.cs            # Steam VDF + Epic JSON + Riot
│   ├── SessionHistoryStore.cs           # JSON-backed history
│   ├── SystemStateChecker.cs            # HAGS/VBS/Hypervisor/Discord/GFE durumu
│   ├── RecommendationsService.cs        # Sistem analizi & öneri üretimi
│   ├── VbsToggler.cs                    # VBS/HVCI registry kapatma
│   ├── HypervisorToggler.cs             # bcdedit hypervisorlaunchtype
│   ├── OverlayDisabler.cs               # Discord + NVIDIA overlay kapatma
│   ├── DnsOptimizer.cs                  # Cloudflare DNS + Discord launcher
│   ├── DpiBypass.cs                     # GoodbyeDPI entegrasyonu
│   ├── WindowsTweaks.cs                 # Background apps + telemetry tasks
│   ├── ShaderCacheCleaner.cs            # D3D/NVIDIA/AMD cache
│   ├── UpdateChecker.cs                 # GitHub Releases API
│   ├── AutoUpdater.cs                   # Self-replace + restart
│   ├── TrayIconHost.cs                  # WinForms NotifyIcon
│   └── SafetyNet.cs                     # Çökme kurtarma
├── Native/
│   ├── NativeMethods.cs                 # OpenProcess, NtSuspend...
│   ├── NtApi.cs                         # Memory list API
│   ├── IconExtractor.cs                 # SHGetFileInfo
│   └── UserInterop.cs                   # SetForegroundWindow
├── ViewModels/
│   └── MainViewModel.cs
├── Views/
│   ├── HistoryWindow.xaml               # Seans geçmişi
│   ├── WhitelistWindow.xaml             # Beyaz liste yöneticisi
│   ├── RecommendationsWindow.xaml       # Sistem önerileri
│   ├── UpdateWindow.xaml                # Otomatik güncelleme
│   └── CloseDialog.xaml                 # Tepside/Çık dialog
├── Resources/
│   ├── FoxMod.ico                       # Uygulama ikonu
│   ├── FoxLogo.png                      # 2400x2400 logo
│   ├── default_game.png                 # Yedek oyun ikonu
│   ├── PresentMon.exe                   # Intel/Microsoft, MIT - gömülü
│   └── GoodbyeDPI/                      # ValdikSS, MIT - gömülü
│       ├── goodbyedpi.exe
│       ├── WinDivert.dll
│       └── WinDivert64.sys
└── MainWindow.xaml
```
</details>

## 🗺 Yol Haritası

### v1.5.x (mevcut)
- [x] PresentMon ile gerçek FPS karşılaştırması
- [x] Otomatik güncelleme (GitHub Releases)
- [x] Sistem Önerileri penceresi
- [x] VBS/HVCI/Hipervizor tek-tık kapatma
- [x] Discord + NVIDIA Overlay kapatma
- [x] Cloudflare DNS optimizasyonu
- [x] GoodbyeDPI DPI bypass (Türkiye Discord erişimi)
- [x] Shader cache temizleme
- [x] Background apps & telemetry tasks
- [x] MMCSS gaming priority + Mouse acceleration off

### v1.6.x (sıradaki — planlanan)
- [ ] **Settings penceresi** — Tüm feature toggle'lar görünür kontrol
- [ ] **Multilingual** (İngilizce / Almanca)
- [ ] **Hotkey desteği** — `Ctrl+Alt+T` ile her yerden Turbo aç/kapa
- [ ] **Real-time FPS overlay (HUD)** — Oyun içinde sol üstte FPS

### v2.0.x (uzun vadeli)
- [ ] **Profil sistemi UI** — Oyun başına özel ayar
- [ ] **Steam header görselleri** — Oyun kartlarında banner
- [ ] **Onboarding turu** — İlk açılış 3 ekran
- [ ] **Code signing sertifikası** — SmartScreen uyarıları kalkar
- [ ] **Plugin sistemi** — Kullanıcı kendi optimizasyon eklesin
- [ ] **Cloud sync** (GitHub Gist)
- [ ] **Web dashboard** — Geçmiş seans analizi

## 🤝 Katkı

Pull request'ler, issue'lar ve fikirler hoş karşılanır.

1. Fork
2. Branch: `git checkout -b feature/awesome`
3. Commit: `git commit -m 'Add awesome feature'`
4. Push: `git push origin feature/awesome`
5. Pull Request aç

İlk katkıların için: README'deki yazım hataları, çevirisi eksik metinler, mimari diyagramdaki yanlışlıklar — küçük ama değerli.

## 💬 Destek

- **💬 Discord**: `peacefox` ([direkt mesaj at](https://discord.com/users/peacefox))
- **🐛 GitHub Issues**: [Sorun bildir / özellik öner](https://github.com/baristilki/foxturbomod/issues)
- **⭐ Beğendiyseniz**: Sağ üstte **Star** vermeyi unutmayın — projenin görünürlüğü için kritik

## 📜 Lisans

**MIT License** — Detay için [LICENSE](LICENSE). Kullanım, modifikasyon, dağıtım serbest. Ticari kullanım dahil.

## 🙏 Üçüncü Taraf

| Bileşen | Lisans | Geliştirici | Kullanım |
|---|---|---|---|
| [PresentMon](https://github.com/GameTechDev/PresentMon) | MIT | Intel / Microsoft | FPS ölçümü (ETW) |
| [GoodbyeDPI](https://github.com/ValdikSS/GoodbyeDPI) | Apache 2.0 | ValdikSS | TLS DPI bypass |
| [WinDivert](https://github.com/basil00/WinDivert) | LGPL | basil00 | WFP packet capture |
| [ModernWpfUI](https://github.com/Kinnara/ModernWpf) | MIT | Kinnara | UI temalama |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | MIT | Microsoft | MVVM altyapı |
| [Microsoft Fluent Emoji](https://github.com/microsoft/fluentui-emoji) | MIT | Microsoft | İkon kaynak |

## 📈 Sürüm Geçmişi

- **v1.5.0** — GoodbyeDPI entegrasyonu, Discord DPI bypass
- **v1.4.0** — DNS optimizasyonu (Cloudflare 1.1.1.1)
- **v1.3.1** — CoD latency fix, "Donduruldu" detay listesi, Mouse accel off, W32Time
- **v1.3.0** — Sistem Önerileri penceresi, VBS/Hipervizor kapatma, Shader cache temizleme
- **v1.2.x** — Otomatik güncelleme, tek instance kilidi, kapat dialog, GitHub entegrasyonu
- **v1.1.x** — Yüklü oyun kütüphane taraması, FPS karşılaştırma paneli, Tepsi ikonu
- **v1.0.0** — İlk kararlı sürüm, Razer Cortex alternatifi olarak yola çıkış

Tam changelog: [Releases](https://github.com/baristilki/foxturbomod/releases)

---

<div align="center">

### Beğendiyseniz ⭐ vermeyi unutmayın!

**Made with 🦊 by [peacefox](https://github.com/baristilki)** · MIT License · 100% open-source

</div>
