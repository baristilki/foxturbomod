# 🦊 Fox Turbo Mod

Windows için açık kaynak oyun performans optimize edici.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue.svg)]()

Oyun açıkken arka planda CPU/RAM tüketen uygulamaları geçici askıya alır, gereksiz Windows servislerini durdurur, sistem cache'ini temizler ve daha akıcı oyun deneyimi sağlar. Oyun kapanınca her şey otomatik geri alınır.

## Ne Yapar

- 🧊 **Süreç askıya alma** (kill etmez!) — OneDrive, Spotify, Chrome sekmeleri, Adobe servisleri vb. 30+ tanımlı süreci dondurur
- 🛑 **Servis durdurma** — Windows Search, SysMain, DiagTrack gibi güvenli 9 servisi geçici kapatır
- 🧠 **Standby memory temizleme** — Windows'un biriktirdiği cache RAM'i temizler (1-3 GB serbest)
- ⚡ **Güç planı** — Yüksek Performans planına geçer, eski plana döner
- 🌐 **Network optimizasyonu** — DNS cache temizler, oyun süreci için DSCP=46 QoS politikası
- 🎬 **Game DVR / Xbox Game Bar kapatma** — %1-3 FPS kazanç, geri alınır
- 🚀 **Oyun süreci için yüksek öncelik** — ProcessPriorityClass.High (Realtime değil, güvenli)
- 🎨 **Görsel efekt minimize** — Pencere animasyonları, gölgeler geçici off
- 🔧 **CPU core parking off** — Tüm çekirdekler aktif
- 🎯 **Gerçek FPS karşılaştırma** — PresentMon (ETW) ile baseline ↔ aktif FPS ölçer
- 📊 **Seans geçmişi** — Her oyun seansını kaydeder, haftalık özet sunar
- 🦊 **Otomatik oyun tespiti** — 50+ oyun (Valorant, LoL, CoD, CS2, BF, Apex, GTA V, EFT, ...) açıldığında Turbo otomatik aktif olur
- 📚 **Steam / Epic / Riot kütüphane taraması** — Yüklü oyunlar ikonları ile listelenir
- 🛡 **Beyaz liste yöneticisi** — Hangi süreçlere dokunulmasın özelleştirilebilir
- 🧰 **Sistem durumu paneli** — HAGS (Hardware GPU Scheduling), DirectStorage, Windows sürümü gösterimi
- 📍 **Tepsi ikonu + arka plan modu** — Pencere kapanınca tepsiye iner, uygulama arka planda çalışmaya devam eder
- 🔄 **GitHub otomatik güncelleme kontrolü** — Açılışta yeni sürüm var mı sorgulanır
- 💾 **Çökme koruması (SafetyNet)** — Uygulama beklenmedik şekilde kapanırsa bir sonraki açılışta donmuş süreçleri otomatik kurtarır

## Ne **Yapmaz**

- ❌ **Cheat değildir** — donanım kimliğine, oyuna, anti-cheat sürücülere dokunmaz
- ❌ Donanım performansı yükseltmez (60 FPS'i 120 yapmaz)
- ❌ DirectX/oyun motoruna dokunmaz
- ❌ PC'ye kalıcı zarar vermez — tüm değişiklikler geri alınabilir
- ❌ Discord, OBS, antivirüse dokunmaz (beyaz listede)
- ❌ Sistem kritik servislerine (RPC, Audio, ağ) dokunmaz

## Gerçek Etkisi

| Senaryo | Beklenen FPS Kazancı |
|---|---|
| 8-16 GB RAM, dağınık masaüstü (Chrome 30+ sekme, OneDrive, Adobe) | %10-25 |
| HDD'li sistem, Search Indexer aktif | %5-15 + stutter azalır |
| 32 GB RAM, az yazılım açık, SSD | %0-3 |
| GPU bottleneck oyun (RT açık Cyberpunk) | %0-2 |

> Fox Turbo Mod, **var olan donanımı verimli kullandırır** — yeni donanım almadan dağınık bir PC'yi 100% verimle çalıştırır.

## Anti-Cheat Uyumluluğu

- ✅ **CoD, BattleField, CS2, Apex, PUBG, EFT, GTA V**: Sorun yok
- ⚠️ **Valorant (Vanguard)**: PresentMon ETW okuduğu için Vanguard "şüpheli telemetri okuyucu" olarak işaretleyebilir. Ban vermez, ama oyun süresince Fox Turbo Mod penceresini kapatmak önerilir. Uygulama arka planda kalır.
- ✅ **League of Legends, Dota 2, Overwatch 2**: Sorun yok

## Kurulum

### Hazır Build (önerilen)
1. [Releases](../../releases) sayfasından son `FoxTurboMod.exe`'yi indir
2. Çift tıkla — Windows SmartScreen "Daha fazla bilgi" → "Yine de çalıştır"
3. UAC penceresinde "Evet" (servis yönetimi için yönetici yetkisi gerek)
4. Hazır

### Kaynak Koddan
```bash
git clone https://github.com/baristilki/foxturbomod
cd fox-turbo-mod
dotnet publish TurboMode/TurboMode.csproj -c Release -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true
```

## Gereksinimler

- Windows 10/11 (64-bit)
- Yönetici yetkisi (servis yönetimi için)
- ~150 MB disk (self-contained build)
- .NET 8 runtime — **gerekmez** (gömülü)

## Kullanım

1. Çalıştır — uygulama tepside açılır, ana pencere ekranda
2. Bir oyun aç (Steam, Epic, Riot vs.)
3. Fox Turbo Mod oyunu otomatik tespit eder ve Turbo modunu açar
4. Oyunda 30 saniye geçtikten sonra FPS karşılaştırma görünür
5. Oyundan çık — her şey otomatik eski haline döner

## Mimari

```
TurboMode/
├── Services/
│   ├── TurboCoordinator.cs       # Tüm optimizer'ları koordine eder
│   ├── ProcessOptimizer.cs       # NtSuspendProcess / NtResumeProcess
│   ├── ServiceOptimizer.cs       # Windows servisleri snapshot+restore
│   ├── MemoryOptimizer.cs        # NtSetSystemInformation cache temizle
│   ├── PowerPlanOptimizer.cs     # powercfg /setactive
│   ├── NetworkOptimizer.cs       # QoS policy + DNS flush
│   ├── GameDvrOptimizer.cs       # Game Bar registry tweaks
│   ├── ProcessPriorityOptimizer.cs
│   ├── VisualEffectsOptimizer.cs
│   ├── CpuParkingOptimizer.cs    # powercfg CPMINCORES
│   ├── FpsMonitor.cs             # PresentMon stdout parse
│   ├── GameDetector.cs           # WMI Win32_ProcessStartTrace
│   ├── GameLibraryScanner.cs     # Steam VDF + Epic JSON + Riot
│   ├── SessionHistoryStore.cs    # JSON-backed history
│   ├── SystemStateChecker.cs     # HAGS + DirectStorage durumu
│   ├── UpdateChecker.cs          # GitHub releases API
│   └── SafetyNet.cs              # Çökme durumunda kurtarma
├── Native/
│   ├── NativeMethods.cs          # OpenProcess, NtSuspend, ...
│   ├── NtApi.cs                  # Memory list API
│   └── IconExtractor.cs          # SHGetFileInfo
├── ViewModels/
│   └── MainViewModel.cs
├── Views/
│   ├── HistoryWindow.xaml
│   └── WhitelistWindow.xaml
├── Resources/
│   ├── FoxMod.ico
│   ├── FoxLogo.png
│   ├── default_game.png
│   └── PresentMon.exe            # gömülü
└── MainWindow.xaml
```

## Güvenli Tasarım Prensipleri

1. **Asla kill, sadece suspend** — process kaybı veya veri kaybı yok
2. **Beyaz liste varsayılan agresif** — Discord, OBS, antivirüs, sistem süreçleri korunur
3. **PID snapshot mantığı** — Sadece *bizim* askıya aldıklarımızı resume ederiz
4. **Servis durumu snapshot+restore** — Zaten kapalıysa açmayız
5. **SafetyNet dosyası** — Çökme durumunda bir sonraki açılışta otomatik kurtarma

## Yol Haritası

- [ ] Onboarding turu (ilk açılış)
- [ ] Toggle açılış animasyonu (sayı animasyonlu)
- [ ] Profil sistemi UI (oyun başına özel ayar)
- [ ] Steam header görselleri (oyun kartlarında banner)
- [ ] Code signing sertifikası (SmartScreen uyarısı yok)
- [ ] Auto-updater integrasyon (Velopack)
- [ ] Tray menüsünden tek tık Turbo aç/kapa
- [ ] Telegram/Discord bot ile geri bildirim

## Destek

💬 **Discord**: `peacefox`

Sorun bildirimi için [GitHub Issues](../../issues) kullanın.

## Lisans

MIT — Detay için [LICENSE](LICENSE).

## Üçüncü Taraf

- **PresentMon** (Intel/Microsoft GameTech) — MIT, [GitHub](https://github.com/GameTechDev/PresentMon)
- **ModernWpfUI** — MIT
- **CommunityToolkit.Mvvm** — MIT
- **Microsoft Fluent Emoji** — MIT (ikon kaynak)
