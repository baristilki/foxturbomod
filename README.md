<div align="center">

<img src="TurboMode/Resources/FoxLogo.png" width="140" alt="Fox Turbo Mod"/>

# 🦊 Fox Turbo Mod v2.0

**Windows için açık kaynak, ücretsiz oyun performans yöneticisi.**

Arka plan süreçlerini askıya alır · Oyun-içi overlay · Gerçek CPU/GPU sıcaklık · Tema desteği · DPI bypass

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue.svg)]()
[![Release](https://img.shields.io/github/v/release/baristilki/foxturbomod?color=orange)](https://github.com/baristilki/foxturbomod/releases)
[![Downloads](https://img.shields.io/github/downloads/baristilki/foxturbomod/total?color=brightgreen)](https://github.com/baristilki/foxturbomod/releases)
[![Stars](https://img.shields.io/github/stars/baristilki/foxturbomod?style=social)](https://github.com/baristilki/foxturbomod/stargazers)

### [📥 İndir](https://github.com/baristilki/foxturbomod/releases/latest) · [💬 Discord](https://discord.com/users/peacefox) · [🐛 Sorun Bildir](https://github.com/baristilki/foxturbomod/issues) · [⭐ Star](https://github.com/baristilki/foxturbomod)

</div>

---

## 🎯 30 Saniyede Özet

Oyun açtığında otomatik devreye girer ve **14+ Windows optimizasyonunu** aynı anda uygular:

```
🧊 Arka plan süreç askıya alma    🛑 Gereksiz Windows servisleri durdurma
🧠 Standby RAM cache temizleme    ⚡ Yüksek Performans güç planı
🎬 Game DVR / Xbox Game Bar off   🚀 Oyun sürecine yüksek öncelik
🎨 Görsel efektleri minimize      🔧 CPU core parking off
🎯 MMCSS gaming priority          🖱 Mouse acceleration off
🌐 Network QoS + DNS flush        💾 Memory standby purge
```

Oyun kapanınca **her şey otomatik geri alınır**. Cheat değildir, donanımı verimli kullandırır.

**v2.0 yeni özellikler**: 📺 Oyun-içi overlay · 🌡 Gerçek CPU/GPU sıcaklık · 🎨 3 tema (Fox/Cyber/Razer) · 🛡 GoodbyeDPI Discord bypass · 💿 Sürücü tarama · 🧹 TEMP cleaner · ⌨ Hotkey customizer

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
- 🧊 **30+ arka plan uygulamasını askıya al** (OneDrive, Spotify, Chrome, Adobe, Slack, Teams)
- 🛑 **9 güvenli Windows servisi durdurma** (WSearch, SysMain, DiagTrack, W32Time, MapsBroker)
- 🧠 **Standby memory cache temizleme** (1-3 GB ekstra RAM)
- ⚡ **Yüksek Performans güç planı** (otomatik geçiş + geri yükleme)
- 🎬 **Game DVR / Xbox Game Bar otomatik kapatma**
- 🚀 **Oyun sürecine `ProcessPriorityClass.High`**
- 🎨 **Görsel efekt minimize** (Aero efektleri geçici kapat)
- 🔧 **CPU core parking off** (tüm çekirdekler aktif)
- 🎯 **MMCSS gaming priority** (multimedia thread öncelik)
- 🖱 **Mouse acceleration off** (gaming standart 1:1)

### 📊 Gerçek Zamanlı Analiz
- 🎯 **PresentMon (Intel/Microsoft)** ile gerçek FPS ölçümü — ETW tabanlı, en doğru
- 🌡 **LibreHardwareMonitor** — gerçek CPU/GPU sıcaklık, kullanım, RAM
- 📈 **Baseline ↔ Aktif FPS karşılaştırması** — "87 → 102 FPS (+17%)"
- 📊 **Seans geçmişi** — Her oyun seansı kaydedilir, haftalık özet
- 💥 **Resource hogs canlı listesi** — En çok RAM yiyen 10 process

### 📺 Oyun-içi Overlay (YENİ — v2.0)
- **Yatay/Dikey/Kare** yerleşim
- **FPS, CPU°, GPU°, RAM** canlı göstergeleri
- **Click-through** mode — mouse oyuna geçer
- **Mouse ile sürüklenebilir** — istediğin konuma taşı, kaydedilir
- **Kilit toggle** (🔓/🔒) + ✕ kapat butonu
- **Hotkey**: Ctrl+Alt+O (aç/kapat), Ctrl+Alt+L (kilit)
- **Yeşil Turbo indicator** — aktif olduğunda yeşil glow dot

### 🎨 3 Tema (YENİ — v2.0)
- 🦊 **Fox** — turuncu, sıcak (varsayılan)
- ⚡ **Cyber** — neon mavi, futurik
- 🎮 **Razer** — neon yeşil, klasik gaming
- **Runtime değişim** — Settings'ten anında

### 🦊 Akıllı Oyun Tespiti
- **50+ oyun**: Valorant, LoL, CS2, CoD, Delta Force, Battlefield 2042, Apex, GTA V, EFT, Cyberpunk, Elden Ring, Helldivers 2, Palworld
- WMI `__InstanceCreationEvent` — uzun process adları desteklenir
- Steam / Epic / Riot kütüphane tarama, oyun ikonlarıyla listele
- Listeden bir oyuna tıkla → direkt başlat

### 🚀 Sistem Önerileri Penceresi
Tek tıkla **direkt aksiyon** butonları:
- 🛑 **VBS / Bellek Bütünlüğü kapat** — %5-15 FPS (Ryzen)
- ⚠ **Hipervizor kapat** — %3-8 FPS (Hyper-V / WSL2 / Docker)
- 🌐 **DNS Cloudflare 1.1.1.1** + Discord aç
- 🛡 **GoodbyeDPI DPI Bypass** + Discord aç (Türkiye)
- 💬 **Discord'u kapat** (overlay'i durdurmak için)
- 🎯 **NVIDIA Overlay'i kapat**
- 🧹 **Shader cache temizle** (D3D/NVIDIA/AMD)
- 💿 **Sürücü tarama** (GPU/Ses/Ağ/Chipset, eski sürücüleri tespit)
- 🧹 **Windows TEMP klasörlerini temizle** (1+ GB tipik)
- 📱 **Background Store uygulamalarını kapat**
- 📡 **Telemetri görevlerini devre dışı bırak**

### ⌨ Hotkey Customizer (YENİ — v2.0)
- 🚀 **Turbo Toggle** (varsayılan: Ctrl+Alt+T)
- 📺 **Overlay Toggle** (Ctrl+Alt+O)
- 🔓 **Overlay Kilit** (Ctrl+Alt+L)
- Settings'te "Düzenle" → tuş yakalama dialog → kaydedildi

### 🛡 Güvenli Tasarım
- **Asla kill, sadece suspend** — Process kaybı yok
- **Beyaz liste agresif**: Discord, OBS, antivirüs, sistem süreçleri dokunulmaz
- **PID snapshot** — Sadece *bizim* askıya aldıklarımızı resume ederiz
- **Servis snapshot+restore** — Zaten kapalıysa açmayız
- **SafetyNet** — Uygulama çökerse otomatik kurtarma
- **Beyaz liste düzenleyici** UI

### 🔄 Otomatik Güncelleme
- Açılışta GitHub Releases sorgulanır
- Yeni sürüm varsa banner çıkar
- **Tek tıkla otomatik indir + kur + yeniden başlat**

### 🎮 Kullanıcı Deneyimi
- 📍 **Tepsi ikonu + arka plan modu**
- ⚡ **Windows başlangıcında otomatik başlat** (YENİ — v2.0)
- 🚪 **Akıllı kapatma diyaloğu** — "Tepside Tut" / "Tamamen Çık"
- 🔒 **Tek instance kilidi**
- ⏱ **Uptime göstergesi**
- 🛡 **Sistem durumu paneli**
- 👋 **Dinamik onboarding** — Sistemini analiz edip 4 kritik aksiyon önerir (YENİ — v2.0)
- 💡 **Tema-uyumlu tooltip kartları**

## 🎮 Anti-Cheat Uyumluluğu

| Oyun / Anti-Cheat | Durum |
|---|---|
| **CoD, Battlefield, CS2, Apex, PUBG, EFT, GTA V** | ✅ Sorun yok |
| **LoL, Dota 2, Overwatch 2, Delta Force (ACE)** | ✅ Sorun yok (düşük risk) |
| **Valorant (Vanguard)** | ⚠ Vanguard çalışırken Fox penceresini kapatmak önerilir (uygulama tepside arka planda kalır). PresentMon ETW + LibreHardwareMonitor kernel driver Vanguard'da uyarı verebilir. **Ban vermez.** |

## 📥 Kurulum

### En kolay yol
1. [**Releases**](https://github.com/baristilki/foxturbomod/releases/latest) sayfasından `FoxTurboMod.exe` indir
2. Çift tıkla → SmartScreen "Daha fazla bilgi" → "Yine de çalıştır"
3. UAC penceresinde **"Evet"** (yönetici yetkisi şart)
4. Onboarding turu açılır → 4 dinamik öneri ile sistem optimize edilir
5. Oyun aç → Fox otomatik devreye girer

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

## 🛠 Gereksinimler
- **Windows 10 / 11** (64-bit)
- **Yönetici yetkisi** (servis yönetimi için zorunlu)
- ~150 MB disk
- **.NET 8 runtime kurulu olmasına GEREK YOK** — gömülü

## ❓ SSS

<details>
<summary><b>Bu cheat mi? Ban yer miyim?</b></summary>

Hayır. Fox Turbo Mod **donanım kimliğine, oyuna veya anti-cheat sürücülere dokunmaz**. Sadece Windows'un kullanıcı seviyesindeki kaynaklarını oyun için yeniden organize eder. Razer Cortex / Process Lasso / Game Mode'un yaptığıyla aynı şey. **Vanguard hariç** tüm AC'lerle uyumlu.
</details>

<details>
<summary><b>Overlay neden görünmüyor?</b></summary>

Oyun **Exclusive Fullscreen** mode'daysa hiçbir overlay üstüne çıkmaz. Çözüm: oyun ayarlarında **Borderless Windowed** veya **Windowed Fullscreen** mode'a al. Modern oyunlarda FPS farkı yok.
</details>

<details>
<summary><b>Discord açılmıyor — Türkiye'de yasaklı?</b></summary>

**🚀 Öneriler** penceresinde 2 çözüm:
- **Adım 1**: Cloudflare DNS'e geç (DNS-bazlı engelleme için)
- **Adım 2**: GoodbyeDPI bypass (TLS SNI/DPI engelleme için)

Her ikisi de yetmezse VPN gerek.
</details>

<details>
<summary><b>FPS ölçüm yanlış değer veriyor</b></summary>

PresentMon **maç içindeki** frame'leri ölçer. Oyun menüsünde veya lobide FPS düşer — bu normal. Maça gir, 30 saniye bekle, gerçek değer akmaya başlar.
</details>

<details>
<summary><b>Geçmiş veriler nerede?</b></summary>

`%LOCALAPPDATA%\FoxMod\` klasöründe:
- `settings.json` — ayarlar
- `history.json` — son 200 seans
- `logs\foxmod-*.log` — günlük log

Hiçbir veri internete gönderilmez.
</details>

## 🏗 Mimari

<details>
<summary>Klasör yapısı</summary>

```
TurboMode/
├── Services/           # 25+ optimizer + servis
├── Native/             # Win32 P/Invoke
├── ViewModels/         # MVVM
├── Views/              # 10+ pencere
├── Themes/             # 3 tema XAML
├── Resources/          # logo, ikon, gömülü PresentMon + GoodbyeDPI
└── MainWindow.xaml
```

Detaylı dosya listesi için [Wiki](https://github.com/baristilki/foxturbomod/wiki) sayfasını oluşturduğumuzda eklenecek.
</details>

## 🗺 Yol Haritası

### v2.0 (mevcut) ✅
- [x] 3 tema runtime değişim
- [x] Oyun-içi overlay
- [x] CPU/GPU sıcaklık (LibreHardwareMonitor)
- [x] Hotkey customizer
- [x] Dinamik onboarding 4 kart
- [x] Driver tarama, TEMP cleaner
- [x] Windows başlangıcında otomatik aç

### v2.1.x (sıradaki — planlanan)
- [ ] **Anti-cheat auto-detect** — Vanguard/ACE'de overlay otomatik kapat
- [ ] **Multilingual** (TR/EN/DE)
- [ ] **FPS spike tespiti** — overlay'de canlı frame time grafiği
- [ ] **Per-game profil UI** — oyun başına özel ayar
- [ ] **Crash log reader** — Windows Event Viewer'dan

### v3.0 (uzun vadeli)
- [ ] **Code signing sertifikası** — SmartScreen uyarısı kalkar
- [ ] **Plugin sistemi** — kullanıcı kendi optimizasyon eklesin
- [ ] **Cloud sync** (GitHub Gist)
- [ ] **Web dashboard** — seans analizi

## 🤝 Katkı

Pull request'ler, issue'lar ve fikirler hoş karşılanır.

1. Fork
2. Branch: `git checkout -b feature/awesome`
3. Commit: `git commit -m 'Add awesome feature'`
4. Push: `git push origin feature/awesome`
5. Pull Request aç

## 💬 Destek
- **💬 Discord**: `peacefox`
- **🐛 GitHub Issues**: [Sorun bildir](https://github.com/baristilki/foxturbomod/issues)
- **⭐ Beğendiyseniz**: Star verin — görünürlük için kritik

## 📜 Lisans
**MIT** — [LICENSE](LICENSE). Kullanım, modifikasyon, dağıtım serbest.

## 🙏 Üçüncü Taraf

| Bileşen | Lisans | Geliştirici |
|---|---|---|
| [PresentMon](https://github.com/GameTechDev/PresentMon) | MIT | Intel / Microsoft |
| [GoodbyeDPI](https://github.com/ValdikSS/GoodbyeDPI) | Apache 2.0 | ValdikSS |
| [WinDivert](https://github.com/basil00/WinDivert) | LGPL | basil00 |
| [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) | MPL 2.0 | LibreHardwareMonitor |
| [ModernWpfUI](https://github.com/Kinnara/ModernWpf) | MIT | Kinnara |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | MIT | Microsoft |
| [Microsoft Fluent Emoji](https://github.com/microsoft/fluentui-emoji) | MIT | Microsoft |
| [SuperTinyIcons](https://github.com/edent/SuperTinyIcons) | MIT | edent |

## 📈 Sürüm Geçmişi

- **v2.0.0** — Overlay, 3 tema, CPU/GPU sıcaklık (LibreHardwareMonitor), hotkey customizer, dinamik onboarding, auto-start, driver tarama, TEMP cleaner, Settings penceresi
- **v1.5.0** — GoodbyeDPI DPI bypass
- **v1.4.0** — DNS optimizasyonu
- **v1.3.x** — Sistem önerileri, VBS/Hipervizor kapatma
- **v1.2.x** — Otomatik güncelleme, tek instance, GitHub entegrasyonu
- **v1.1.x** — Kütüphane tarama, FPS karşılaştırma, tepsi
- **v1.0.0** — İlk sürüm

---

<div align="center">

### Beğendiyseniz ⭐ vermeyi unutmayın!

**Made with 🦊 by [peacefox](https://github.com/baristilki)** · MIT License · 100% open-source

</div>
