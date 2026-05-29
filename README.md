<div align="center">

<img src="TurboMode/Resources/FoxLogo.png" width="120" alt="Fox Turbo Mod"/>

# Fox Turbo Mod

**Windows için açık kaynak, ücretsiz oyun performans yöneticisi.**
Arka plan süreçleri askıya alır · Gerçek FPS ölçer · Tek tıkla sistem optimize eder

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue.svg)]()
[![Release](https://img.shields.io/github/v/release/baristilki/foxturbomod?color=orange)](https://github.com/baristilki/foxturbomod/releases)
[![Downloads](https://img.shields.io/github/downloads/baristilki/foxturbomod/total?color=brightgreen)](https://github.com/baristilki/foxturbomod/releases)

[**📥 İndir**](https://github.com/baristilki/foxturbomod/releases/latest) ·
[**💬 Discord**](https://discord.com/users/peacefox) ·
[**🐛 Sorun Bildir**](https://github.com/baristilki/foxturbomod/issues)

</div>

---

## 🎯 Bu Nedir?

Fox Turbo Mod, Razer Cortex'in açık kaynak alternatifidir. Oyun açtığında:

1. Arka planda CPU/RAM tüketen 30+ uygulamayı **askıya alır** (kill etmez!)
2. Gereksiz Windows servislerini durdurur
3. Cache temizler, güç planını yüksek performansa alır
4. Game DVR'ı kapatır, oyun sürecine yüksek öncelik verir
5. Görsel efektleri minimize eder, CPU core parking'i durdurur
6. MMCSS gaming priority + network throttle off uygular
7. **PresentMon ile gerçek FPS karşılaştırması** sunar

Oyun kapandığında her şey **otomatik geri alınır**. Cheat değil — donanımı verimli kullandırır.

## ✨ Öne Çıkan Özellikler

### 🚀 Optimizasyonlar (otomatik)
| Özellik | Etki | Mekanizma |
|---|---|---|
| Süreç askıya alma | %10-25 FPS | `NtSuspendProcess` (30+ tanımlı süreç) |
| Servis durdurma | Disk I/O azalır | `WSearch`, `SysMain`, `DiagTrack` + 6 servis |
| Standby memory cache | 1-3 GB RAM serbest | `NtSetSystemInformation` |
| Güç planı | Latency düşer | `powercfg /setactive` High Performance |
| Network QoS | Ping stabilizasyonu | `New-NetQosPolicy` DSCP=46 + DNS flush |
| Game DVR kapatma | %1-3 FPS | 5 registry değeri snapshot+restore |
| Oyun process'i priority | Frame stutter↓ | `ProcessPriorityClass.High` |
| Görsel efekt minimize | %2-3 FPS | Registry `VisualFXSetting=2` |
| CPU core parking off | %3-5 FPS | `powercfg CPMINCORES 100` |
| MMCSS gaming priority | %2-5 FPS | `SystemResponsiveness=0` + Games task |
| Network throttle off | Network stutter↓ | `NetworkThrottlingIndex=0xFFFFFFFF` |

### 📊 Gerçek FPS Ölçümü
- **PresentMon (Intel/Microsoft)** gömülü — ETW (Event Tracing for Windows) ile gerçek frame time
- Baseline FPS (Turbo kapalı) ↔ Aktif FPS (Turbo açık) karşılaştırma
- Her oyun seansı kaydedilir → haftalık özet (`+14% ortalama FPS, 47 saat aktif, 8.3 GB serbest`)

### 🦊 Akıllı Oyun Tespiti
- **50+ oyun** otomatik tespit (Valorant, LoL, CS2, CoD, Delta Force, BF, Apex, GTA V, EFT, ...)
- WMI `__InstanceCreationEvent` ile **uzun process adları desteklenir**
- Steam / Epic / Riot kütüphane tarama → tüm yüklü oyunlar ikonlarıyla listede

### 🛡 Güvenli Tasarım
- **Asla kill, sadece suspend** — process kaybı yok
- **Beyaz liste agresif**: Discord, OBS, antivirüs, sistem süreçleri **dokunulmaz**
- **PID snapshot mantığı**: Sadece *bizim* askıya aldıklarımızı resume ederiz
- **Servis snapshot+restore**: Zaten kapalıysa açmayız
- **SafetyNet**: Uygulama çökerse bir sonraki açılışta otomatik kurtarma

### 🚀 Sistem Önerileri Penceresi
Tek tıkla **kapatma** aksiyonları:
- 🛑 **VBS / Bellek Bütünlüğü kapat** — %5-15 FPS (Ryzen)
- ⚠ **Hipervizor kapat** — %3-8 FPS (Hyper-V/WSL2/Docker)
- 💬 **Discord kapat** (overlay'i durdurur) — %3-7 FPS
- 🎯 **NVIDIA Overlay kapat** — %1-3 FPS
- 🧹 **Shader Cache temizle** — stutter azaltır
- 📱 **Arka Plan Store uygulamalarını kapat**
- 📡 **Telemetri görevlerini devre dışı bırak**
- 🎮 **NVIDIA Control Panel direkt aç** (Max Performance modu)

### 🔄 Otomatik Güncelleme
- Açılışta GitHub Releases sorgulanır
- Yeni sürüm varsa tek tıkla **otomatik indir + kur + yeniden başlat**

## 📈 Gerçek Etkisi

| Senaryo | Beklenen Kazanç |
|---|---|
| 8-16 GB RAM, dağınık masaüstü (Chrome 30+ sekme, OneDrive, Adobe) | **%15-30 FPS** |
| HDD'li sistem, Search Indexer aktif | %5-15 + stutter azalır |
| 32 GB RAM, SSD, az yazılım | %3-8 |
| GPU bottleneck oyun (RT Cyberpunk) | %0-2 |

> **Var olan donanımı verimli kullandırır** — yeni donanım almadan dağınık bir PC'yi optimize eder.

## 🎮 Anti-Cheat Uyumluluğu

| Oyun / AC | Durum |
|---|---|
| CoD, BattleField, CS2, Apex, PUBG, EFT, GTA V | ✅ Sorun yok |
| LoL, Dota 2, Overwatch 2, Delta Force (ACE) | ✅ Sorun yok |
| **Valorant (Vanguard)** | ⚠ Vanguard çalışırken Fox Turbo Mod penceresini kapatmak önerilir (uygulama arka planda kalır). PresentMon ETW'yi Vanguard "şüpheli telemetri" olarak işaretleyebilir. **Ban vermez.** |

## 📥 Kurulum

### En kolay yol (önerilen)
1. [**Releases**](https://github.com/baristilki/foxturbomod/releases/latest) sayfasından **`FoxTurboMod.exe`**'yi indir
2. Çift tıkla → Windows SmartScreen "Daha fazla bilgi" → "Yine de çalıştır"
3. UAC penceresinde **"Evet"** (servis yönetimi için yönetici yetkisi gerek)
4. Bir oyun aç → Fox Turbo Mod otomatik devreye girer

### Kaynak koddan derleme
```bash
git clone https://github.com/baristilki/foxturbomod
cd foxturbomod
dotnet publish TurboMode/TurboMode.csproj -c Release -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true
```

Çıktı: `bin/Release/net8.0-windows/win-x64/publish/FoxTurboMod.exe`

## 🛠 Gereksinimler
- Windows 10/11 (64-bit)
- Yönetici yetkisi (servis yönetimi için zorunlu)
- ~150 MB disk
- **.NET 8 runtime kurulu olmasına gerek YOK** — gömülü (self-contained)

## ⚠️ Bilinen Uyarılar

| Uyarı | Sebep | Çözüm |
|---|---|---|
| Windows SmartScreen "Bilinmeyen yayıncı" | Code signing yok | "Daha fazla bilgi" → "Yine de çalıştır" |
| Chrome "yaygın olarak indirilmiyor" | Yeni + imzasız exe | "Tut" / "Yine de indir" |
| Microsoft Family Safety bloğu | Aile kısıtlamaları | Aile yöneticisi `account.microsoft.com/family`'den izin verir |

> Bu uyarılar **kalıcı code signing sertifikası** (~$200/yıl) alınınca tamamen kaybolur. Açık kaynak proje olarak henüz finanse edilmedi.

## 🏗 Mimari

```
TurboMode/
├── Services/
│   ├── TurboCoordinator.cs           # Tüm optimizer'ları koordine eder
│   ├── ProcessOptimizer.cs           # NtSuspendProcess / NtResumeProcess
│   ├── ServiceOptimizer.cs           # Windows servisleri snapshot+restore
│   ├── MemoryOptimizer.cs            # NtSetSystemInformation cache temizle
│   ├── PowerPlanOptimizer.cs         # powercfg /setactive
│   ├── NetworkOptimizer.cs           # QoS policy + DNS flush
│   ├── GameDvrOptimizer.cs           # Game Bar registry tweaks
│   ├── ProcessPriorityOptimizer.cs   # Process priority class
│   ├── VisualEffectsOptimizer.cs     # VisualFXSetting registry
│   ├── CpuParkingOptimizer.cs        # powercfg CPMINCORES
│   ├── SystemResponsivenessOptimizer.cs  # MMCSS gaming priority
│   ├── FpsMonitor.cs                 # PresentMon stdout parse
│   ├── GameDetector.cs               # WMI __InstanceCreationEvent
│   ├── GameLibraryScanner.cs         # Steam VDF + Epic JSON + Riot
│   ├── SessionHistoryStore.cs        # JSON-backed history
│   ├── SystemStateChecker.cs         # HAGS, VBS, Hypervisor, Discord, GFE durumu
│   ├── RecommendationsService.cs     # Sistem önerileri analizi
│   ├── VbsToggler.cs                 # VBS/HVCI registry kapatma
│   ├── HypervisorToggler.cs          # bcdedit hypervisorlaunchtype
│   ├── OverlayDisabler.cs            # Discord + NVIDIA overlay kapatma
│   ├── WindowsTweaks.cs              # Background apps + telemetry tasks
│   ├── ShaderCacheCleaner.cs         # D3D/NVIDIA/AMD cache
│   ├── UpdateChecker.cs              # GitHub Releases API
│   ├── AutoUpdater.cs                # Self-replace + restart
│   ├── TrayIconHost.cs               # WinForms NotifyIcon
│   └── SafetyNet.cs                  # Çökme kurtarma
├── Native/
│   ├── NativeMethods.cs              # OpenProcess, NtSuspend, ...
│   ├── NtApi.cs                      # Memory list API
│   ├── IconExtractor.cs              # SHGetFileInfo
│   └── UserInterop.cs                # SetForegroundWindow, ShowWindow
├── ViewModels/
│   └── MainViewModel.cs              # MVVM ana state
├── Views/
│   ├── HistoryWindow.xaml            # Seans geçmişi
│   ├── WhitelistWindow.xaml          # Beyaz liste yöneticisi
│   ├── RecommendationsWindow.xaml    # Sistem önerileri
│   ├── UpdateWindow.xaml             # Otomatik güncelleme
│   └── CloseDialog.xaml              # Tepside/Çık dialog
├── Resources/
│   ├── FoxMod.ico
│   ├── FoxLogo.png
│   ├── default_game.png
│   └── PresentMon.exe                # gömülü (Intel/Microsoft, MIT)
└── MainWindow.xaml
```

## 🗺 Yol Haritası

- [x] PresentMon ile gerçek FPS karşılaştırması
- [x] Otomatik güncelleme (GitHub Releases)
- [x] Sistem Önerileri penceresi
- [x] VBS/HVCI kapatma butonu
- [x] Hipervizor kapatma butonu
- [x] Discord & NVIDIA Overlay kapatma
- [x] Background apps & telemetry tasks
- [x] Shader cache temizleme
- [x] Tek instance kilidi
- [ ] Onboarding turu (ilk açılış)
- [ ] Toggle açılış animasyonu
- [ ] Profil sistemi UI (oyun başına özel ayar)
- [ ] Steam header görselleri (oyun kartlarında banner)
- [ ] Code signing sertifikası (SmartScreen uyarısı yok)
- [ ] FPS karşılaştırması grafik görselleştirme
- [ ] Tray menüsünden tek tık Turbo aç/kapa
- [ ] Settings penceresi (tüm feature toggle'lar)

## 🤝 Katkı

Pull request'ler, issue'lar ve fikirler hoş karşılanır.

1. Fork edin
2. Branch oluşturun: `git checkout -b feature/awesome`
3. Commit edin: `git commit -m 'Add awesome feature'`
4. Push edin: `git push origin feature/awesome`
5. Pull Request açın

## 💬 Destek

- **Discord**: `peacefox`
- **GitHub Issues**: [Sorun Bildir](https://github.com/baristilki/foxturbomod/issues)

## 📜 Lisans

MIT — Detay için [LICENSE](LICENSE). Kullanım, modifikasyon ve dağıtım serbest.

## 🙏 Üçüncü Taraf

| Bileşen | Lisans | Kullanım |
|---|---|---|
| [PresentMon](https://github.com/GameTechDev/PresentMon) (Intel/Microsoft) | MIT | FPS ölçümü |
| [ModernWpfUI](https://github.com/Kinnara/ModernWpf) | MIT | UI tema |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | MIT | MVVM altyapı |
| [Microsoft Fluent Emoji](https://github.com/microsoft/fluentui-emoji) | MIT | İkon kaynak |

---

<div align="center">

**Beğendiyseniz ⭐ vermeyi unutmayın!**

Made with 🦊 by [peacefox](https://github.com/baristilki)

</div>
