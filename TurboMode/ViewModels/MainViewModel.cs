using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TurboMode.Models;
using TurboMode.Services;
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;

namespace TurboMode.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private TurboCoordinator _coordinator;
    private readonly GameDetector _detector;
    private readonly SystemMetrics _metrics;
    private readonly GameLibraryScanner _scanner = new();
    private readonly FpsMonitor _fps = new();
    public FpsMonitor Fps => _fps;
    private readonly HardwareMonitor _hw = new();
    public ResourceHogsMonitor Hogs { get; } = new();

    public AppSettings Settings { get; private set; }

    private double _baselineCpu;
    private double _baselineRamUsedGb;
    private double _latestCpu;
    private double _latestRamUsedGb;
    private double _latestRamTotalGb;
    private double _baselineFps;
    private double _activeFps;
    private string? _currentGameExe;
    private SessionRecord? _currentSession;
    private ActivationResult? _lastResult;

    public IReadOnlyList<string> LastSuspendedProcesses =>
        _lastResult?.SuspendedProcessNames ?? Array.Empty<string>();
    public IReadOnlyList<string> LastStoppedServices =>
        _lastResult?.StoppedServiceNames ?? Array.Empty<string>();

    [ObservableProperty] private bool _turboEnabled;
    [ObservableProperty] private string _statusTitle = "Hazır";
    [ObservableProperty] private string _statusDetail = "Açmak için sağdaki düğmeyi kullanın. Oyun açıldığında otomatik devreye alınır.";
    [ObservableProperty] private string _gameStatus = "Şu anda oyun çalışmıyor";
    [ObservableProperty] private Brush _gameStatusColor = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x98));
    [ObservableProperty] private string _cpuLabel = "0%";
    [ObservableProperty] private double _cpuPercent;
    [ObservableProperty] private string _ramLabel = "0 / 0 GB";
    [ObservableProperty] private double _ramPercent;
    [ObservableProperty] private string _libraryHeader = "Yüklü Oyunlar";
    [ObservableProperty] private string _libraryStatusText = "Taranıyor...";
    [ObservableProperty] private string _footerText = "Fox Turbo Mod • Değişiklikler oyun kapanınca otomatik geri alınır.";
    [ObservableProperty] private string _versionLabel =
        "v" + (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?.?.?");
    [ObservableProperty] private string _uptimeLabel = "🕒 Aktif: 0 dk";
    // Network ping (4 hedef)
    [ObservableProperty] private string _pingCloudflare = "—";
    [ObservableProperty] private string _pingGoogle = "—";
    [ObservableProperty] private string _pingDiscord = "—";
    [ObservableProperty] private string _pingSteam = "—";

    // Hardware sensörleri
    [ObservableProperty] private string _cpuName = "CPU";
    [ObservableProperty] private string _cpuTempLabel = "—";
    [ObservableProperty] private double _cpuTemp;
    [ObservableProperty] private double _cpuLoad;
    [ObservableProperty] private Brush _cpuTempColor = new SolidColorBrush(Color.FromRgb(0x7F, 0xD4, 0x9F));
    [ObservableProperty] private string _gpuName = "GPU";
    [ObservableProperty] private string _gpuTempLabel = "—";
    [ObservableProperty] private double _gpuTemp;
    [ObservableProperty] private double _gpuLoad;
    [ObservableProperty] private Brush _gpuTempColor = new SolidColorBrush(Color.FromRgb(0x7F, 0xD4, 0x9F));
    private readonly DateTime _appStartedAt = DateTime.Now;
    [ObservableProperty] private string _weeklySummaryText = "Henüz veri yok";
    [ObservableProperty] private string _updateBanner = "";
    public string? UpdateUrl { get; private set; }
    public UpdateChecker.UpdateInfo? PendingUpdate { get; private set; }

    [ObservableProperty] private Visibility _resultPanelVisibility = Visibility.Collapsed;
    [ObservableProperty] private string _resultSuspendedText = "0";
    [ObservableProperty] private string _resultRamFreedText = "0 MB";
    [ObservableProperty] private string _resultCpuDeltaText = "0% → 0%";
    [ObservableProperty] private string _resultExtrasText = "";

    [ObservableProperty] private Visibility _fpsPanelVisibility = Visibility.Collapsed;
    [ObservableProperty] private string _fpsLiveText = "—";
    [ObservableProperty] private string _fpsBaselineText = "—";
    [ObservableProperty] private string _fpsActiveText = "—";
    [ObservableProperty] private string _fpsDeltaText = "Veri toplanıyor...";
    [ObservableProperty] private Brush _fpsDeltaColor = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x98));

    // Sistem durumu (HAGS, Win11, DirectStorage)
    [ObservableProperty] private string _systemStateLabel = "Sistem kontrol ediliyor...";
    [ObservableProperty] private Brush _systemStateColor = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x98));

    [ObservableProperty] private bool _aboutOpen;

    // Overlay aktif mi (📺 buton rengi için)
    [ObservableProperty] private bool _overlayActive;
    [ObservableProperty] private Brush _overlayButtonBg = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
    [ObservableProperty] private Brush _overlayButtonBorder = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x33));
    [ObservableProperty] private Brush _overlayButtonFg = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));

    partial void OnOverlayActiveChanged(bool value)
    {
        if (value)
        {
            OverlayButtonBg = new SolidColorBrush(Color.FromRgb(0x2A, 0x1F, 0x14));
            OverlayButtonBorder = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x29));
            OverlayButtonFg = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x29));
        }
        else
        {
            OverlayButtonBg = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            OverlayButtonBorder = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x33));
            OverlayButtonFg = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
        }
    }

    public ObservableCollection<InstalledGame> InstalledGames { get; } = new();
    public IRelayCommand ToggleAboutCommand { get; }

    public MainViewModel()
    {
        Settings = ProfileStore.Load();
        _metrics = new SystemMetrics();
        _coordinator = new TurboCoordinator(Settings);
        _detector = new GameDetector();

        _metrics.Updated += (cpu, ramUsedGb, ramTotalGb) =>
        {
            _latestCpu = cpu;
            _latestRamUsedGb = ramUsedGb;
            _latestRamTotalGb = ramTotalGb;
            CpuPercent = cpu;
            CpuLabel = $"{cpu:0}%";
            RamPercent = ramTotalGb > 0 ? ramUsedGb / ramTotalGb * 100.0 : 0;
            RamLabel = $"{ramUsedGb:0.0} / {ramTotalGb:0.0} GB";
        };
        _metrics.Start();

        _detector.GameStarted += OnGameStarted;
        _detector.GameStopped += OnGameStopped;
        _detector.Start();

        // Açılışta zaten çalışan oyunu UI'da göster + FPS monitor'u başlat (Turbo otomatik açılmaz)
        if (!string.IsNullOrEmpty(_detector.CurrentGame))
        {
            GameStatus = $"Algılandı: {_detector.CurrentGame}";
            GameStatusColor = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x29));
            // FPS Monitor başlat — kullanıcı oyuna girmiş, ölçmeye başla
            var procName = _detector.CurrentGameProcess;
            if (Settings.EnableFpsMonitor && !string.IsNullOrEmpty(procName) && _fps.Start(procName))
            {
                FpsPanelVisibility = Visibility.Visible;
                FpsLiveText = "ölçülüyor...";
                Log.Info("FPS Monitor başlatıldı (açılış-zamanı tespit): {0}", procName);
            }
        }

        // FPS UI'sı için ayrı timer
        var fpsUiTimer = new DispatcherTimer(DispatcherPriority.Background)
        { Interval = TimeSpan.FromMilliseconds(500) };
        int debugTickCounter = 0;
        fpsUiTimer.Tick += (_, _) =>
        {
            string newText;
            if (!_fps.IsRunning) newText = "—";
            else
            {
                var avg = _fps.AverageFps(3);
                newText = avg > 5 ? $"{avg:0} FPS" : "ölçülüyor...";
            }
            FpsLiveText = newText;
            // Her 10 saniyede bir UI değerini log'a yaz (debug)
            if (++debugTickCounter % 20 == 0)
                Log.Info($"UI FpsLiveText set: '{newText}' (FpsMonitor IsRunning={_fps.IsRunning}, sampleCount={_fps.SampleCount})");
        };
        fpsUiTimer.Start();

        ToggleAboutCommand = new RelayCommand(() => AboutOpen = !AboutOpen);
        _ = ScanLibraryAsync();
        _ = Task.Run(CheckSystemState);
        _ = Task.Run(CheckUpdateAsync);
        RefreshWeeklySummary();
        StartUptimeTimer();

        _hw.Updated += h =>
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CpuName = ShortenChip(h.CpuName);
                if (h.CpuTempC > 0) { CpuTemp = h.CpuTempC; CpuTempLabel = $"{h.CpuTempC:0}°"; CpuTempColor = TempColor(h.CpuTempC); }
                else CpuTempLabel = "—";
                CpuLoad = h.CpuLoadPercent;

                GpuName = ShortenChip(h.GpuName);
                if (h.GpuTempC > 0) { GpuTemp = h.GpuTempC; GpuTempLabel = $"{h.GpuTempC:0}°"; GpuTempColor = TempColor(h.GpuTempC); }
                else GpuTempLabel = "—";
                GpuLoad = h.GpuLoadPercent;
            });
        };
        _hw.Start();
        Hogs.Start();
        StartNetworkPing();
    }

    private void StartNetworkPing()
    {
        var t = new DispatcherTimer(DispatcherPriority.Background)
        { Interval = TimeSpan.FromSeconds(15) };
        t.Tick += async (_, _) => await ProbeNetwork();
        t.Start();
        _ = ProbeNetwork();
    }

    private async Task ProbeNetwork()
    {
        try
        {
            var results = await NetworkProbe.ProbeAsync();
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var r in results)
                {
                    var txt = r.Success ? $"{r.PingMs} ms" : "✗";
                    switch (r.Target)
                    {
                        case "Cloudflare DNS": PingCloudflare = txt; break;
                        case "Google DNS": PingGoogle = txt; break;
                        case "Discord": PingDiscord = txt; break;
                        case "Steam": PingSteam = txt; break;
                    }
                }
            });
        }
        catch (Exception ex) { Log.Error(ex, "MainVM ProbeNetwork hatası"); }
    }

    private static string ShortenChip(string name)
    {
        return name
            .Replace("Intel(R) ", "").Replace("Intel ", "")
            .Replace("AMD ", "")
            .Replace("NVIDIA GeForce ", "")
            .Replace(" CPU", "").Replace("(R)", "").Replace("(TM)", "").Trim();
    }

    private static Brush TempColor(float t) => t switch
    {
        >= 85 => new SolidColorBrush(Color.FromRgb(0xE0, 0x40, 0x40)),
        >= 75 => new SolidColorBrush(Color.FromRgb(0xCC, 0x99, 0x33)),
        _ => new SolidColorBrush(Color.FromRgb(0x7F, 0xD4, 0x9F)),
    };

    private void StartUptimeTimer()
    {
        UpdateUptimeLabel();
        var t = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        t.Tick += (_, _) => UpdateUptimeLabel();
        t.Start();
    }

    private void UpdateUptimeLabel()
    {
        var span = DateTime.Now - _appStartedAt;
        if (span.TotalHours >= 1)
            UptimeLabel = $"🕒 Aktif: {(int)span.TotalHours} sa {span.Minutes} dk";
        else if (span.TotalMinutes >= 1)
            UptimeLabel = $"🕒 Aktif: {span.Minutes} dk";
        else
            UptimeLabel = $"🕒 Aktif: {span.Seconds} sn";
    }

    private void RefreshWeeklySummary()
    {
        try
        {
            var (count, avg, ram, time) = SessionHistoryStore.WeeklySummary();
            if (count == 0)
            {
                WeeklySummaryText = "Henüz oyun kaydı yok";
                return;
            }
            WeeklySummaryText = $"{count} oyun • +{avg:0.0}% FPS • {ram} MB";
        }
        catch { WeeklySummaryText = "—"; }
    }

    private async Task CheckUpdateAsync()
    {
        var info = await UpdateChecker.CheckAsync();
        if (info == null) return;
        if (info.IsNewer)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                UpdateBanner = $"  • v{info.LatestVersion} mevcut — Güncelle ↗";
                UpdateUrl = info.ReleaseUrl;
                PendingUpdate = info;
            });
        }
    }

    private void CheckSystemState()
    {
        var state = SystemStateChecker.Check();
        var notes = new List<string>();
        var warn = false;
        if (!state.HagsSupported) notes.Add("HAGS: desteklenmiyor");
        else if (state.HagsEnabled) notes.Add("HAGS: ✓ aktif");
        else { notes.Add("HAGS: ✗ kapalı"); warn = true; }
        if (state.DirectStorageSupported) notes.Add("DirectStorage: ✓ desteklenir");
        else notes.Add("DirectStorage: × Win11 gerekir");
        notes.Add(state.WindowsVersion);
        App.Current.Dispatcher.Invoke(() =>
        {
            SystemStateLabel = string.Join("  •  ", notes);
            SystemStateColor = new SolidColorBrush(warn
                ? Color.FromRgb(0xCC, 0xAA, 0x44)
                : Color.FromRgb(0x90, 0x90, 0x98));
        });
    }

    public void ReloadSettings()
    {
        Settings = ProfileStore.Load();
        _coordinator = new TurboCoordinator(Settings);
    }

    private async Task ScanLibraryAsync()
    {
        var games = await Task.Run(() => _scanner.Scan());
        App.Current.Dispatcher.Invoke(() =>
        {
            InstalledGames.Clear();
            foreach (var g in games) InstalledGames.Add(g);
            LibraryStatusText = games.Count == 0
                ? "Yüklü oyun bulunamadı"
                : $"{games.Count} oyun bulundu";
        });
    }

    partial void OnTurboEnabledChanged(bool value)
    {
        if (value)
        {
            _baselineCpu = _latestCpu;
            _baselineRamUsedGb = _latestRamUsedGb;
            _baselineFps = _fps.AverageFps(20);

            var result = _coordinator.Activate(_detector.CurrentGameProcess, _currentGameExe);
            _lastResult = result;

            StatusTitle = "AKTİF";
            StatusDetail = "Tüm optimizasyonlar uygulandı. Sonuçlar 3 sn sonra güncellenir.";

            ResultSuspendedText = $"{result.SuspendedProcesses} süreç + {result.StoppedServices} servis";
            ResultRamFreedText = result.RamFreedMb > 0
                ? $"{result.RamFreedMb} MB (cache temizliği)"
                : "ölçülüyor...";
            ResultCpuDeltaText = "ölçülüyor...";

            var extras = new List<string>();
            if (result.PowerPlanSwitched) extras.Add("⚡ Yüksek Performans");
            if (result.NetworkOptimized) extras.Add("🌐 QoS + DNS");
            if (result.GameDvrDisabled) extras.Add("🎬 Game DVR off");
            if (result.PriorityBoosted) extras.Add("🚀 Yüksek Öncelik");
            if (result.VisualEffectsMinimized) extras.Add("🎨 Görsel efekt min");
            if (result.CpuParkingDisabled) extras.Add("🔧 CPU parking off");
            if (result.MmcssBoosted) extras.Add("🎯 MMCSS gaming");
            if (result.MouseTweaked) extras.Add("🖱 Mouse accel off");
            if (Settings.EnableMemoryCleanup) extras.Add("🧠 Cache temiz");
            ResultExtrasText = string.Join("  •  ", extras);

            ResultPanelVisibility = Visibility.Visible;

            if (_baselineFps > 5)
            {
                FpsBaselineText = $"{_baselineFps:0} FPS";
                FpsActiveText = "ölçülüyor (30 sn)...";
                FpsDeltaText = "30 sn bekleyin";
                FpsDeltaColor = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x98));
            }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (!TurboEnabled) return;
                double ramFreedGb = Math.Max(0, _baselineRamUsedGb - _latestRamUsedGb);
                int totalFreedMb = (int)(ramFreedGb * 1024) + (int)result.RamFreedMb;
                ResultRamFreedText = totalFreedMb > 50
                    ? $"{totalFreedMb} MB serbest"
                    : "fark yok (sistem zaten boştu)";
                ResultCpuDeltaText = $"{_baselineCpu:0}% → {_latestCpu:0}%";
            };
            timer.Start();

            if (_baselineFps > 5)
            {
                var fpsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
                fpsTimer.Tick += (_, _) =>
                {
                    fpsTimer.Stop();
                    if (!TurboEnabled) return;
                    _activeFps = _fps.AverageFps(20);
                    if (_activeFps < 5) { FpsActiveText = "yetersiz veri"; return; }
                    FpsActiveText = $"{_activeFps:0} FPS";
                    double pct = (_activeFps - _baselineFps) / _baselineFps * 100.0;
                    FpsDeltaText = pct >= 0 ? $"+{pct:0.0}% kazanç 🎉" : $"{pct:0.0}% (etki yok)";
                    FpsDeltaColor = pct > 2
                        ? new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x29))
                        : new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x98));
                };
                fpsTimer.Start();
            }
        }
        else
        {
            _coordinator.Deactivate();
            StatusTitle = "Hazır";
            StatusDetail = "Tüm sistem ayarları geri yüklendi.";
            ResultPanelVisibility = Visibility.Collapsed;
        }
    }

    private void OnGameStarted(string gameName, string processName)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            GameStatus = $"Algılandı: {gameName}";
            GameStatusColor = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x29));
            foreach (var g in InstalledGames)
            {
                g.IsRunning = string.Equals(g.ProcessName, processName, StringComparison.OrdinalIgnoreCase);
                if (g.IsRunning) _currentGameExe = g.ExecutablePath;
            }

            _currentSession = new SessionRecord
            {
                StartedUtc = DateTime.UtcNow,
                GameName = gameName,
                GameProcess = processName,
            };

            if (Settings.EnableFpsMonitor && _fps.Start(processName))
            {
                FpsPanelVisibility = Visibility.Visible;
                FpsLiveText = "ölçülüyor...";
                FpsBaselineText = "—";
                FpsActiveText = "—";
                FpsDeltaText = "Toggle açtığınızda karşılaştırılacak";
            }

            if (Settings.AutoActivateOnGameStart && !TurboEnabled) TurboEnabled = true;
        });
    }

    private void OnGameStopped(string gameName)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            GameStatus = "Şu anda oyun çalışmıyor";
            GameStatusColor = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x98));
            foreach (var g in InstalledGames) g.IsRunning = false;

            // Seansı kaydet (eğer 30+ saniye sürdüyse — yanlışlıkla açılan oyunları sayma)
            if (_currentSession != null)
            {
                _currentSession.EndedUtc = DateTime.UtcNow;
                if (_currentSession.Duration.TotalSeconds >= 30)
                {
                    _currentSession.SuspendedProcesses = _lastResult?.SuspendedProcesses ?? 0;
                    _currentSession.StoppedServices = _lastResult?.StoppedServices ?? 0;
                    _currentSession.RamFreedMb = _lastResult?.RamFreedMb ?? 0;
                    _currentSession.BaselineFps = _baselineFps;
                    _currentSession.ActiveFps = _activeFps;
                    SessionHistoryStore.Append(_currentSession);
                    RefreshWeeklySummary();
                }
                _currentSession = null;
            }

            _currentGameExe = null;
            _baselineFps = 0;
            _activeFps = 0;
            _fps.Stop();
            FpsPanelVisibility = Visibility.Collapsed;
            // Oyun bittiyse açık overlay'i de kapat
            try
            {
                foreach (var w in System.Windows.Application.Current.Windows.OfType<Views.OverlayWindow>().ToList())
                    w.Close();
                OverlayActive = false;
            }
            catch { }
        });
    }
}
