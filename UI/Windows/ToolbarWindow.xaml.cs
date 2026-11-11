using Engine.Characters;
using Engine.Enums;
using Engine.Models;
using Engine.References.Models;
using Engine.Services.Implements;
using Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using UI.Helpers;
using UI.Models;
using UI.Services;
using UI.Windows;
using DrawingColor = System.Drawing.Color;

namespace UI.Windows
{
    /// <summary>
    /// Floating toolbar that controls the OverlayWindow, SubRegionConfigWindow, and ItemListWindow.
    /// </summary>
    public partial class ToolbarWindow : Window
    {
        private readonly IScreenScanner _screenScanner;
        private readonly IDetector _colorItemDetector;
        private readonly IItemCluster _itemCluster;
        private readonly IMoveStep _moveStep;
        private readonly IHealthBarScanner _healthBarScanner;
        private readonly IMiniMapScanner _miniMapScanner;
        private readonly IAction _action;
        private readonly DispatcherTimer _scanTimer;

        /// <summary>
        /// Trạng thái tự động Farm khi phát hiện tài nguyên trong Mini-map (phím P).
        /// </summary>
        public bool IsFarming { get; set; } = false;
        private int _noResourceCount = 0;
        private double _lastNearestDeltaX = 0;
        private double _lastNearestDeltaY = 0;
        private double _lastNearestDistance = 0;
        private int _stationaryScanCount = 0;
        private DateTime _lastFarmPressTime = DateTime.MinValue;

        // ─── Sub-Region Dedicated Pipeline Timers ────────────────────────
        private readonly Dictionary<int, DispatcherTimer> _subRegionTimers = new();
        private readonly SemaphoreSlim _healthBarLock = new(1, 1);
        private readonly SemaphoreSlim _miniMapLock = new(1, 1);
        private readonly SemaphoreSlim _enemyLock = new(1, 1);
        private bool _isCombatActive = false;
        private bool _isEnergyBlocked = false;
        private readonly IEnemyScanner _enemyScanner;
        private readonly IChestScanner _chestScanner;
        private readonly SemaphoreSlim _chestLock = new(1, 1);

        /// <summary>
        /// Temporary test mode: When true, only Chest (R6) pipeline scans periodically (every 1s),
        /// while all other pipelines skip early without performing actions.
        /// Set to false to restore normal full automation.
        /// </summary>
        public bool IsTestMode { get; set; } = true;

        /// <summary>
        /// Player main character state tracked by scanning pipelines.
        /// </summary>
        public Main MainCharacter { get; } = new Main();

        // ─── Grid size cycling ────────────────────────────────────────────
        private static readonly int[] GridSteps = { 10, 20, 30, 40, 50 };
        private int _gridStepIndex = 0;   // starts at 10×10

        // ─── Setting for scan ─────────────────
        private double _timerInterval = 5000; // milliseconds (5s)
        private bool _isScanning = false;

        // ─── Colours used for active-state buttons ────────────────────────
        private static readonly SolidColorBrush BrushNormal = new(Color.FromArgb(42, 255, 255, 255));
        private static readonly SolidColorBrush BrushActive = new(Color.FromArgb(80, 0, 230, 118));
        private static readonly SolidColorBrush BorderNormal = new(Color.FromArgb(48, 255, 255, 255));
        private static readonly SolidColorBrush BorderActive = new(Color.FromArgb(170, 0, 230, 118));

        // ─── References ──────────────────────────────────────────────────
        private readonly OverlayWindow _overlay;
        private readonly ItemListWindow _itemList;
        private readonly SubRegionConfigWindow _subRegionConfig;
        private readonly TerminalLogWindow _terminalLog;
        private bool _itemListVisible = false;
        private bool _subRegionConfigVisible = false;
        private bool _terminalLogVisible = false;

        // ─── Win32 Global Hotkey (F2) ────────────────────────────────────
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int HOTKEY_ID_F2 = 0x9002;
        private const int HOTKEY_ID_ESC = 0x9003;
        private const uint VK_ESCAPE = 0x1B; // ESC key
        private const uint MOD_NONE = 0x0000;
        private const uint VK_F2 = 0x71; // F2 key
        private const int WM_HOTKEY = 0x0312;

        // ─── Constructor ──────────────────────────────────────────────────
        public ToolbarWindow(
            OverlayWindow overlay,
            IScreenScanner screenScanner,
            IDetector? colorItemDetector = null,
            IItemCluster? itemCluster = null,
            IMoveStep? moveStep = null,
            IHealthBarScanner? healthBarScanner = null,
            IMiniMapScanner? miniMapScanner = null,
            IEnemyScanner? enemyScanner = null,
            IChestScanner? chestScanner = null,
            IAction? action = null)
        {
            InitializeComponent();

            _overlay = overlay;
            _itemList = new ItemListWindow();
            _subRegionConfig = new SubRegionConfigWindow(overlay);
            _terminalLog = new TerminalLogWindow();
            _screenScanner = screenScanner;
            _miniMapScanner = miniMapScanner ?? new MiniMapScanner();
            _enemyScanner = enemyScanner ?? new EnemyScanner();
            _chestScanner = chestScanner ?? new ChestScanner();
            _action = action ?? new Engine.Services.Implements.Action();

            // ─── Set Z-Order: Toolbar is stacked above Overlay ───────────────────
            try
            {
                if (overlay != null && overlay.IsVisible)
                {
                    Owner = overlay;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Toolbar Owner Warning]: {ex.Message}");
            }

            // ─── Exclude All Tool UI From Screen Captures ─────────────────────────
            // Even if a tool window is accidentally dragged over MR or any R,
            // screen captures (CopyFromScreen) capture the game beneath, never the UI.
            WindowHelper.ExcludeFromCapture(this);
            WindowHelper.ExcludeFromCapture(_overlay);
            WindowHelper.ExcludeFromCapture(_subRegionConfig);
            WindowHelper.ExcludeFromCapture(_itemList);
            WindowHelper.ExcludeFromCapture(_terminalLog);

            _terminalLog.IsVisibleChanged += (s, e) =>
            {
                if (!_terminalLog.IsVisible)
                {
                    _terminalLogVisible = false;
                    BtnTerminalLog.Background = BrushNormal;
                    BtnTerminalLog.BorderBrush = BorderNormal;
                }
            };

            // Dùng mặc định nếu không inject từ ngoài vào
            _colorItemDetector = colorItemDetector ?? new Detector();
            _itemCluster = itemCluster ?? new ItemCluster();
            _moveStep = moveStep ?? new MoveStep();
            _healthBarScanner = healthBarScanner ?? new HealthBarScanner();

            // Panels follow toolbar when it moves
            LocationChanged += OnToolbarRepositioned;
            SizeChanged += OnToolbarRepositioned;

            // Place toolbar on the right edge once layout is complete
            Loaded += OnLoaded;

            // ─── Khởi tạo Main MR Timer ──────────────────────────────────────
            _scanTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_timerInterval)
            };
            _scanTimer.Tick += OnScanTimerTick;

            // ─── Khởi tạo các Pipeline Timer cho từng Sub-Region (R1..R5) ─────
            InitializeSubRegionPipelines();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = (HwndSource?)PresentationSource.FromVisual(this);
            source?.AddHook(WndProc);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (handle != IntPtr.Zero)
            {
                RegisterHotKey(handle, HOTKEY_ID_F2, MOD_NONE, VK_F2);
                RegisterHotKey(handle, HOTKEY_ID_ESC, MOD_NONE, VK_ESCAPE);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == HOTKEY_ID_F2)
                {
                    ToggleClickThrough();
                    handled = true;
                }
                else if (id == HOTKEY_ID_ESC)
                {
                    PauseScanning();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F2)
            {
                ToggleClickThrough();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                PauseScanning();
                e.Handled = true;
            }
        }

        private void InitializeSubRegionPipelines()
        {
            foreach (var region in _overlay.SubRegions)
            {
                bool hasInterval = region.HasInterval || (IsTestMode && region.Id == 6);
                if (!hasInterval) continue;
                int rId = region.Id;
                int intervalMs = (IsTestMode && rId == 6) ? 1000 : Math.Max(50, region.ScanIntervalMs);
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(intervalMs)
                };
                timer.Tick += (s, e) => OnSubRegionTimerTick(rId);
                _subRegionTimers[rId] = timer;
            }

            // Đồng bộ chu kỳ quét khi người dùng thay đổi trên thanh Sub-Region Taskbar
            _overlay.SubRegionsUpdated += (s, e) =>
            {
                foreach (var region in _overlay.SubRegions)
                {
                    if (_subRegionTimers.TryGetValue(region.Id, out var timer))
                    {
                        TimeSpan newInterval = TimeSpan.FromMilliseconds(Math.Max(50, region.ScanIntervalMs));
                        if (timer.Interval != newInterval)
                        {
                            timer.Interval = newInterval;
                        }
                    }
                }
            };
        }

        // ─── Auto-place on right edge of screen & Safe Owner Setup ──────
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var work = SystemParameters.WorkArea;
            Left = work.Right - ActualWidth - 12;
            Top = work.Top + (work.Height - ActualHeight) / 2;

            // Now that Toolbar is shown, safely attach ownership of child windows
            try
            {
                _subRegionConfig.Owner = this;
                _itemList.Owner = this;
                _terminalLog.Owner = this;

                if (_overlay.HasLoadedConfig)
                {
                    _terminalLog.Log(0, "[Config] Restored saved positions and settings from config.json.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Child Window Owner Setup Warning]: {ex.Message}");
            }
        }

        // ─── Drag to move ─────────────────────────────────────────────────
        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // ─── Keep Child Windows Anchored to Toolbar ───────────────────────
        private void OnToolbarRepositioned(object? sender, EventArgs e)
        {
            PositionPanels();
        }

        private void PositionPanels()
        {
            var work = SystemParameters.WorkArea;

            // If toolbar is near right edge, dock panels to the left of toolbar; otherwise to the right
            bool dockLeft = (Left + ActualWidth + 320 > work.Right);

            double currentX = dockLeft ? Left : (Left + ActualWidth);

            if (_subRegionConfigVisible && !_subRegionConfig.IsCustomPositioned)
            {
                double panelW = _subRegionConfig.ActualWidth > 0 ? _subRegionConfig.ActualWidth : 300;
                if (dockLeft)
                {
                    currentX -= (panelW + 6);
                    _subRegionConfig.Left = currentX;
                }
                else
                {
                    currentX += 6;
                    _subRegionConfig.Left = currentX;
                    currentX += panelW;
                }
                _subRegionConfig.Top = Top;
            }

            if (_itemListVisible && !_itemList.IsCustomPositioned)
            {
                double panelW = _itemList.ActualWidth > 0 ? _itemList.ActualWidth : 260;
                if (dockLeft)
                {
                    currentX -= (panelW + 6);
                    _itemList.Left = currentX;
                }
                else
                {
                    currentX += 6;
                    _itemList.Left = currentX;
                }
                _itemList.Top = Top;
            }

            if (_terminalLogVisible && !_terminalLog.IsCustomPositioned)
            {
                double panelW = _terminalLog.ActualWidth > 0 ? _terminalLog.ActualWidth : 540;
                if (dockLeft)
                {
                    currentX -= (panelW + 6);
                    _terminalLog.Left = currentX;
                }
                else
                {
                    currentX += 6;
                    _terminalLog.Left = currentX;
                    currentX += panelW;
                }
                _terminalLog.Top = Top;
            }
        }

        // ─── Timer Tick Handler: Tự động chạy mỗi _timerInterval ───────────
        private async void OnScanTimerTick(object? sender, EventArgs e)
        {
            if (!_isScanning) return;
            await ExecuteScanPipelineAsync();
        }

        // ─── Pipeline Quét Màn Hình (Vòng lặp quét MR: kiểm tra Enemy & Mini Map) ───
        private async Task ExecuteScanPipelineAsync()
        {
            if (IsTestMode) return;
            try
            {
                // 1. Kiểm tra Enemy trước: Nếu phát hiện quái -> Dừng farming, giữ phím F tấn công
                bool hasEnemy = await CheckEnemyCombatPipelineAsync();
                if (hasEnemy || _isEnergyBlocked)
                {
                    return;
                }

                // 2. Không có enemy -> Tiếp tục vòng lặp quét Mini Map & Auto Farm
                await CheckMiniMapFarmingAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Scan Error]: {ex.Message}");
            }
        }

        // ─── Button 1 : Toggle Grid ───────────────────────────────────────
        private void BtnToggleGrid_Click(object sender, RoutedEventArgs e)
        {
            _overlay.ToggleGrid();

            bool active = _overlay.IsGridVisible;
            BtnToggleGrid.Background = active ? BrushActive : BrushNormal;
            BtnToggleGrid.BorderBrush = active ? BorderActive : BorderNormal;
        }

        // ─── Button 2 : Cycle Grid Size ───────────────────────────────────
        private void BtnGridSize_Click(object sender, RoutedEventArgs e)
        {
            _gridStepIndex = (_gridStepIndex + 1) % GridSteps.Length;
            int n = GridSteps[_gridStepIndex];
            _overlay.SetGridSize(n, GetCenterIgnoredRadius(n));
            GridSizeLabel.Text = $"{n}×{n}";
        }

        private int GetCenterIgnoredRadius(int gridN)
        {
            if (gridN <= 15) return 1;
            if (gridN <= 25) return 2;
            if (gridN <= 35) return 3;
            if (gridN <= 45) return 4;
            return 5;
        }

        // ─── Button : Sub-Regions Control (Zones) ─────────────────────────
        private void BtnSubRegions_Click(object sender, RoutedEventArgs e)
        {
            _subRegionConfigVisible = !_subRegionConfigVisible;

            if (_subRegionConfigVisible)
            {
                if (!_subRegionConfig.IsCustomPositioned) PositionPanels();
                _subRegionConfig.RefreshUI();
                _subRegionConfig.Show();
                BtnSubRegions.Background = BrushActive;
                BtnSubRegions.BorderBrush = BorderActive;
            }
            else
            {
                SaveConfiguration();
                _subRegionConfig.Hide();
                BtnSubRegions.Background = BrushNormal;
                BtnSubRegions.BorderBrush = BorderNormal;
                PositionPanels();
            }
        }

        // ─── Button : Click-Through / Play Mode Toggle ────────────────────
        private Button? ClickThroughBtn => FindName("BtnClickThrough") as Button;
        private TextBlock? ClickThroughIcon => FindName("TxtClickThroughIcon") as TextBlock;
        private TextBlock? ClickThroughLabel => FindName("TxtClickThroughLabel") as TextBlock;

        private void BtnClickThrough_Click(object sender, RoutedEventArgs e)
        {
            ToggleClickThrough();
        }

        private void ToggleClickThrough()
        {
            bool nextState = !_overlay.IsClickThrough;
            _overlay.SetClickThrough(nextState);

            var btn = ClickThroughBtn;
            var icon = ClickThroughIcon;
            var lbl = ClickThroughLabel;

            if (nextState)
            {
                if (btn != null)
                {
                    btn.Background = BrushActive;
                    btn.BorderBrush = BorderActive;
                    btn.ToolTip = "Click-Through is ON (Play Mode) [F2]: Clicks pass directly into game. Click or press F2 to edit MR/zones.";
                }
                if (icon != null)
                {
                    icon.Text = "🎮";
                    icon.Foreground = Brushes.LightGreen;
                }
                if (lbl != null)
                {
                    lbl.Text = "Play ON";
                }

                // Update configuration file when locked (Play Mode activated)
                SaveConfiguration();

                _terminalLog.Log(0, "[Click-Through] Play Mode ACTIVATED: Mouse clicks now pass through into the game. Press [F2] or click Toolbar to edit.");
            }
            else
            {
                if (btn != null)
                {
                    btn.Background = BrushNormal;
                    btn.BorderBrush = BorderNormal;
                    btn.ToolTip = "Click-Through is OFF (Edit Mode) [F2]: You can drag/resize MR and scan zones. Click or press F2 to pass clicks into game.";
                }
                if (icon != null)
                {
                    icon.Text = "🖱️";
                    icon.Foreground = new SolidColorBrush(Color.FromRgb(64, 196, 255));
                }
                if (lbl != null)
                {
                    lbl.Text = "Click-Thru";
                }
                _terminalLog.Log(0, "[Click-Through] Edit Mode ACTIVATED: You can now drag and resize MR and Sub-Regions.");
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                bool success = ConfigService.Save(_overlay, _itemList);
                if (success)
                {
                    _terminalLog.Log(0, "[Config] Saved configuration to config.json (MR & Sub-Regions).");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Config] Save error: {ex.Message}");
            }
        }

        // ─── Button 3 : Toggle Item List ──────────────────────────────────
        private void BtnItemList_Click(object sender, RoutedEventArgs e)
        {
            _itemListVisible = !_itemListVisible;

            if (_itemListVisible)
            {
                if (!_itemList.IsCustomPositioned) PositionPanels();
                _itemList.Show();
                BtnItemList.Background = BrushActive;
                BtnItemList.BorderBrush = BorderActive;
            }
            else
            {
                _itemList.Hide();
                BtnItemList.Background = BrushNormal;
                BtnItemList.BorderBrush = BorderNormal;
                PositionPanels();
            }
        }

        // ─── Button : Pipeline Terminal Logs ──────────────────────────────
        private void BtnTerminalLog_Click(object sender, RoutedEventArgs e)
        {
            _terminalLogVisible = !_terminalLogVisible;

            if (_terminalLogVisible)
            {
                if (!_terminalLog.IsCustomPositioned) PositionPanels();
                _terminalLog.Show();
                BtnTerminalLog.Background = BrushActive;
                BtnTerminalLog.BorderBrush = BorderActive;
            }
            else
            {
                _terminalLog.Hide();
                BtnTerminalLog.Background = BrushNormal;
                BtnTerminalLog.BorderBrush = BorderNormal;
                PositionPanels();
            }
        }

        // ─── Button 4 : Start ─────────────────────────────────────────────
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _isScanning = true;
            BtnStart.Background = BrushActive;
            BtnPause.Background = BrushNormal;

            // Start Main MR pipeline timer
            _scanTimer.Start();

            // Start each Sub-Region pipeline timer
            foreach (var timer in _subRegionTimers.Values)
            {
                timer.Start();
            }

            _terminalLog.Log(0, "Scanning started. Pipelines active.");

            // Immediately run an initial scan pass
            if (IsTestMode)
            {
                _terminalLog.SelectTab(6);
                _ = ExecuteSubRegionPipelineAsync(6);
            }
            else
            {
                _ = ExecuteScanPipelineAsync();
                _ = ExecuteSubRegionPipelineAsync(1); // R1: Healthbar pipeline
            }
        }

        // â”€â”€â”€ Button 5 : Pause â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            PauseScanning();
        }

        public void PauseScanning()
        {
            _isScanning = false;
            _isCombatActive = false;
            _action.StopHoldingAttack();
            BtnPause.Background = BrushActive;
            BtnStart.Background = BrushNormal;

            _scanTimer.Stop();
            foreach (var timer in _subRegionTimers.Values)
            {
                timer.Stop();
            }

            _terminalLog.Log(0, "Scanning paused via [ESC] / Pause button.");
        }

        // â”€â”€â”€ Button 6 : Close â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            StopAllTimers();
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveConfiguration();

            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                if (handle != IntPtr.Zero)
                {
                    UnregisterHotKey(handle, HOTKEY_ID_F2);
                    UnregisterHotKey(handle, HOTKEY_ID_ESC);
                }
            }
            catch { }

            base.OnClosed(e);
            StopAllTimers();
            _overlay.Close();
            _subRegionConfig.Close();
            _itemList.Close();
            _terminalLog.Close();
        }

        private void StopAllTimers()
        {
            _isScanning = false;
            _isCombatActive = false;
            _action.StopHoldingAttack();
            _scanTimer.Stop();
            foreach (var timer in _subRegionTimers.Values)
            {
                timer.Stop();
            }
        }

        // ─── Sub-Region Pipeline Handler ───────────────────────────────────
        private async void OnSubRegionTimerTick(int regionId)
        {
            if (!_isScanning) return;
            await ExecuteSubRegionPipelineAsync(regionId);
        }

        private async Task ExecuteSubRegionPipelineAsync(int regionId)
        {
            if (!_isScanning) return;

            var region = _overlay.SubRegions.FirstOrDefault(r => r.Id == regionId);
            if (region == null || !region.IsVisible) return;

            if (IsTestMode)
            {
                if (regionId == 6)
                {
                    await ExecuteChestPipelineAsync(region);
                }
                return;
            }

            switch (regionId)
            {
                case 1:
                    await ExecuteHealthBarPipelineAsync(region);
                    break;
                case 2:
                    await ExecuteMiniMapPipelineAsync(region);
                    break;
                case 3:
                    ExecuteInventoryPipeline(region);
                    break;
                case 4:
                    ExecuteEquipmentPipeline(region);
                    break;
                case 5:
                    await ExecuteEnemyBarPipelineAsync(region);
                    break;
                case 6:
                    await ExecuteChestPipelineAsync(region);
                    break;
            }
        }

        // ─── R1: Healthbar Scan Pipeline (Text OCR & Health Object Model) ─
        private async Task ExecuteHealthBarPipelineAsync(SubRegion region)
        {
            // Avoid overlapping OCR scans if the previous frame is still processing
            if (!await _healthBarLock.WaitAsync(0))
                return;

            try
            {
                Rect boundsBar = _overlay.GetSubRegionScreenBounds(1);
                Rect boundsText = _overlay.GetHealthBarTextScreenBounds();

                if (boundsBar.IsEmpty || boundsBar.Width <= 4 || boundsBar.Height <= 4 ||
                    boundsText.IsEmpty || boundsText.Width <= 4 || boundsText.Height <= 4)
                    return;

                int barX = (int)Math.Round(boundsBar.X);
                int barY = (int)Math.Round(boundsBar.Y);
                int barW = (int)Math.Round(boundsBar.Width);
                int barH = (int)Math.Round(boundsBar.Height);

                int textX = (int)Math.Round(boundsText.X);
                int textY = (int)Math.Round(boundsText.Y);
                int textW = (int)Math.Round(boundsText.Width);
                int textH = (int)Math.Round(boundsText.Height);

                PlayerHealthInfo info = await _healthBarScanner.ScanSeparateRegionsFromScreenAsync(
                    barX, barY, barW, barH,
                    textX, textY, textW, textH,
                    region.MaxHealthValue,
                    region.HealThresholdPercent);

                // Update Player main character model in Engine
                MainCharacter.HealthInfo = info;

                // Format concise real-time log: text: % | heathBar: % | final: %
                string textDisplay = (info.OcrCurrentHp >= 0 && info.TextPercentage >= 0)
                    ? $"{info.TextPercentage:F0}%"
                    : "N/A";

                // Ensure engine action has current region cooldown
                Engine.Services.Implements.Action.HealCooldownMs = region.HealCooldownMs;

                // Call Quick Heal action when health is low (at or below configured threshold)
                bool healTriggered = false;
                if (info.FinalHealthPercentage <= region.HealThresholdPercent && info.FinalHealthPercentage > 0)
                {
                    healTriggered = _action.Heal();
                    info.HealTriggered = healTriggered;
                }

                string actionLog = healTriggered ? " | action: heal (press: H)" : "";
                string logMessage = $"text: {textDisplay} | heathBar: {info.VisualBarPercentage:F0}% | final: {info.FinalHealthPercentage:F0}%{actionLog}";

                _terminalLog.Log(1, logMessage);

                Debug.WriteLine($"[R1 Healthbar Pipeline]: {logMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HealthBar Pipeline Error]: {ex.Message}");
            }
            finally
            {
                _healthBarLock.Release();
            }
        }

        // ─── R2: Mini Map Pipeline ────────────────────────────────────────
        private async Task ExecuteMiniMapPipelineAsync(SubRegion region)
        {
            await CheckMiniMapFarmingAsync(region);
        }

        private async Task CheckMiniMapFarmingAsync(SubRegion? region = null)
        {
            if (_isCombatActive || _isEnergyBlocked) return;
            if (!await _miniMapLock.WaitAsync(0))
                return;

            try
            {
                region ??= _overlay.SubRegions.FirstOrDefault(r => r.Id == 2);
                if (region == null || !region.IsVisible) return;

                Rect bounds = _overlay.GetSubRegionScreenBounds(2);
                if (bounds.IsEmpty || bounds.Width <= 10 || bounds.Height <= 10) return;

                int sx = (int)Math.Round(bounds.X);
                int sy = (int)Math.Round(bounds.Y);
                int sw = (int)Math.Round(bounds.Width);
                int sh = (int)Math.Round(bounds.Height);

                var detected = await _miniMapScanner.ScanObjectsFromScreenAsync(sx, sy, sw, sh);
                bool hasResource = (detected != null && detected.Any());
                bool isMoving = false;
                string actionLog = "";

                if (hasResource)
                {
                    _noResourceCount = 0;
                    var nearest = detected!.First();

                    if (!IsFarming)
                    {
                        IsFarming = true;
                        isMoving = false;
                        _lastNearestDeltaX = nearest.DeltaX;
                        _lastNearestDeltaY = nearest.DeltaY;
                        _lastNearestDistance = nearest.DistanceFromCenter;
                        _stationaryScanCount = 0;

                        _ = Task.Run(() => _action.AutoFarm());
                        actionLog = " | action: farm (press: P)";
                    }
                    else
                    {
                        double diffX = Math.Abs(nearest.DeltaX - _lastNearestDeltaX);
                        double diffY = Math.Abs(nearest.DeltaY - _lastNearestDeltaY);

                        bool isStationary = (diffX <= 2.0 && diffY <= 2.0);
                        isMoving = !isStationary;

                        if (isStationary)
                        {
                            _stationaryScanCount++;
                            if (_stationaryScanCount >= 3)
                            {
                                _stationaryScanCount = 0;
                                _lastNearestDeltaX = nearest.DeltaX;
                                _lastNearestDeltaY = nearest.DeltaY;
                                _lastNearestDistance = nearest.DistanceFromCenter;

                                _ = Task.Run(() => _action.AutoFarm());
                                actionLog = " | action: farm (press: P retry)";
                            }
                        }
                        else
                        {
                            _lastNearestDeltaX = nearest.DeltaX;
                            _lastNearestDeltaY = nearest.DeltaY;
                            _lastNearestDistance = nearest.DistanceFromCenter;
                            _stationaryScanCount = 0;
                        }
                    }
                }
                else
                {
                    _stationaryScanCount = 0;
                    isMoving = false;

                    _noResourceCount++;
                    bool shouldStopFarm = (_noResourceCount >= 3);
                    if (shouldStopFarm) _noResourceCount = 0;
                    if (shouldStopFarm && IsFarming)
                    {
                        IsFarming = false;
                        _lastFarmPressTime = DateTime.UtcNow;
                        _ = Task.Run(() => _action.AutoFarm());


                        actionLog = " | action: stop farm (press: P)";
                    }
                }

                _terminalLog.Log(2, $"farming: {IsFarming} | moving: {isMoving} | resource found: {hasResource}{actionLog}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MiniMap Check Error]: {ex.Message}");
            }
            finally
            {
                _miniMapLock.Release();
            }
        }

        private void ExecuteInventoryPipeline(SubRegion region)
        {
            // Dedicated inventory items scanning logic
        }

        // ─── R4: Equipment Pipeline ───────────────────────────────────────
        private void ExecuteEquipmentPipeline(SubRegion region)
        {
            // Dedicated equipment gear scanning logic
        }

        // â”€â”€â”€ R6: Chest / Container Pipeline â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async Task ExecuteChestPipelineAsync(SubRegion region)
        {
            if (!await _chestLock.WaitAsync(0)) return;
            try
            {
                Rect bounds = _overlay.GetSubRegionScreenBounds(6);
                if (bounds.IsEmpty || bounds.Width <= 20 || bounds.Height <= 20) return;

                int cX = (int)Math.Round(bounds.X);
                int cY = (int)Math.Round(bounds.Y);
                int cW = (int)Math.Round(bounds.Width);
                int cH = (int)Math.Round(bounds.Height);

                await ProcessChestLootingAsync(cX, cY, cW, cH);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Chest Pipeline Error]: {ex.Message}");
            }
            finally
            {
                _chestLock.Release();
            }
        }

        private async Task<bool> ProcessChestLootingAsync(int cX, int cY, int cW, int cH)
        {
            int pass = 0;
            var allItems = ItemsRegistry.GetDefaultItems();

            while (_isScanning && pass < 5)
            {
                pass++;
                bool isOpen = _chestScanner.IsChestOpen(cX, cY, cW, cH);
                _terminalLog.Log(6, isOpen ? "chest: OPEN" : "chest: CLOSED");
                if (!isOpen)
                {
                    return false;
                }

                var slots = _chestScanner.ScanChestSlots(cX, cY, cW, cH, allItems);
                if (slots == null || slots.Count == 0)
                {
                    _terminalLog.Log(6, "chest summary: all 25 slots empty (no items to loot) -> closing chest (press: X)");
                    await Task.Delay(250);
                    _action.PressX();
                    await Task.Delay(350);
                    return true;
                }

                var found = slots.Where(s => s.HasItem && s.Item != null && s.Item.Name != ItemName.Unknown).ToList();
                if (!found.Any())
                {
                    if (pass == 1)
                    {
                        _terminalLog.Log(6, "chest summary: all 25 slots empty (no items to loot) -> closing chest (press: X)");
                    }
                    else
                    {
                        _terminalLog.Log(6, "chest summary: all items looted/cleared, verified empty -> closing chest (press: X)");
                    }
                    await Task.Delay(250);
                    _action.PressX();
                    await Task.Delay(350);
                    return true;
                }

                // Log danh sÃ¡ch cÃ¡c item quÃ©t Ä‘Æ°á»£c trong lÆ°á»£t nÃ y
                string itemsStr = string.Join(", ", found.Select(s => $"{s.Row + 1}x{s.Col + 1}: {s.ItemDisplayName}"));
                int emptyCount = slots.Count - found.Count;
                //_terminalLog.Log(6, $"{itemsStr} ... {emptyCount} slots empty (pass {pass})");

                var selectedItems = _itemList.GetSelectedItems();

                for (int i = 0; i < found.Count; i++)
                {
                    if (!_isScanning) break;

                    var s = found[i];

                    // TÃ­nh tá»a Ä‘á»™ chÃ­nh giá»¯a cá»§a Ã´ slot dá»±a trÃªn tá»· lá»‡ lÆ°á»›i 5x5
                    double cellW = (double)cW / 5.0;
                    double cellH = (double)cH / 5.0;
                    int centerX = (int)Math.Round(cX + (s.Col + 0.5) * cellW);
                    int centerY = (int)Math.Round(cY + (s.Row + 0.5) * cellH);

                    bool isEnabled = selectedItems.Any(sel => sel.Name == s.Item.Name || (!string.IsNullOrEmpty(sel.DisplayName) && sel.DisplayName == s.Item.DisplayName));

                    if (isEnabled)
                    {
                        _terminalLog.Log(6, $"Slot {s.Row + 1}x{s.Col + 1}: {s.ItemDisplayName} (Enabled) -> Double-click to loot");
                        _action.DoubleClickAt(centerX, centerY);
                        await Task.Delay(350);
                    }
                    else
                    {
                        _terminalLog.Log(6, $"Slot {s.Row + 1}x{s.Col + 1}: {s.ItemDisplayName} (Disabled) -> Selected, pressed R. Pausing loot until [O] is pressed...");
                        _action.ClickAt(centerX, centerY);
                        await Task.Delay(250);
                        _action.PressR();
                        await Task.Delay(500);
                        _action.PressO();

                        await Task.Delay(700);
                    }

                    // Khi loot/xá»­ lÃ½ tá»›i Ã´ cuá»‘i cÃ¹ng, thÃ´ng bÃ¡o chuáº©n bá»‹ quÃ©t láº¡i Ä‘á»ƒ Ä‘áº£m báº£o Ä‘Ã£ láº¥y háº¿t
                    if (i == found.Count - 1)
                    {
                        _terminalLog.Log(6, "loot: reached last item slot -> rescanning chest to verify all items taken...");
                    }
                }

                // Äá»‡m thá»i gian giá»¯a cÃ¡c lÆ°á»£t nháº·t trÆ°á»›c khi quÃ©t láº¡i
                await Task.Delay(400);
            }

            // Äáº£m báº£o Ä‘Ã³ng rÆ°Æ¡ng náº¿u káº¿t thÃºc chu ká»³ quÃ©t mÃ  rÆ°Æ¡ng váº«n Ä‘ang má»Ÿ
            if (_chestScanner.IsChestOpen(cX, cY, cW, cH))
            {
                _terminalLog.Log(6, "loot: finished loot cycle -> closing chest (press: X)");
                _action.PressX();
                await Task.Delay(350);
            }

            return true;
        }

        // â”€â”€â”€ R5: Enemy Bar & Attached Energy Pipeline â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async Task ExecuteEnemyBarPipelineAsync(SubRegion region)
        {
            await CheckEnemyCombatPipelineAsync(region);
        }

        private async Task<bool> CheckEnemyCombatPipelineAsync(SubRegion? enemyRegion = null)
        {
            if (!await _enemyLock.WaitAsync(0))
                return false;

            try
            {
                enemyRegion ??= _overlay.SubRegions.FirstOrDefault(r => r.Id == 5);
                if (enemyRegion == null || !enemyRegion.IsVisible) return false;

                Rect enemyBounds = _overlay.GetSubRegionScreenBounds(5);
                Rect energyBounds = _overlay.GetEnemyEnergyTextScreenBounds();

                if (enemyBounds.IsEmpty || enemyBounds.Width <= 4 || enemyBounds.Height <= 4)
                    return false;

                int enemyX = (int)Math.Round(enemyBounds.X);
                int enemyY = (int)Math.Round(enemyBounds.Y);
                int enemyW = (int)Math.Round(enemyBounds.Width);
                int enemyH = (int)Math.Round(enemyBounds.Height);

                int energyX = (int)Math.Round(energyBounds.X);
                int energyY = (int)Math.Round(energyBounds.Y);
                int energyW = (int)Math.Round(energyBounds.Width);
                int energyH = (int)Math.Round(energyBounds.Height);

                var info = await _enemyScanner.ScanEnemyAndEnergyFromScreenAsync(
                    enemyX, enemyY, enemyW, enemyH,
                    energyX, energyY, energyW, energyH,
                    enemyRegion.MaxEnergyValue);

                string energyDisplay = info.CurrentEnergy >= 0 ? $"{info.CurrentEnergy}%" : "N/A";

                // If energy bar detected text number -> Block C and Block P (F is unblocked)
                bool hasEnergyNumber = (info.CurrentEnergy >= 0);
                _isEnergyBlocked = hasEnergyNumber;
                Engine.Services.Implements.Action.BlockC = hasEnergyNumber;
                Engine.Services.Implements.Action.BlockAutoFarm = hasEnergyNumber;

                if (hasEnergyNumber && IsFarming)
                {
                    IsFarming = false;
                    _terminalLog.Log(2, $"farming: {IsFarming} | moving: false | resource found: false");
                }

                info.HasEnemy = info.EnemyHpPercentage >= enemyRegion.EnemyThresholdPercent;

                if (info.HasEnemy)
                {
                    // Priority: hold F attack until enemy is defeated (F is never blocked)
                    bool wasFarming = IsFarming;
                    IsFarming = false;
                    _isCombatActive = true; // Pause/stop current scan pipeline throughout combat and looting

                    while (_isScanning)
                    {
                        _action.StartHoldingAttack();
                        _terminalLog.Log(5, $"enemy: true (hp: {info.EnemyHpPercentage:F0}%) | energy: {energyDisplay} | action: attack (holding F)");

                        while (_isScanning)
                        {
                            await Task.Delay(200);

                            bool enemyStillPresent = _enemyScanner.CheckEnemyRedBarFromScreen(enemyX, enemyY, enemyW, enemyH);
                            if (!enemyStillPresent)
                            {
                                break;
                            }
                        }

                        _action.StopHoldingAttack();
                        _terminalLog.Log(5, $"enemy: false | energy: {energyDisplay}");

                        // Stop current scan pipeline for about 2s before pressing C
                        // While waiting ~2s, verify no more enemies appear (if any appear, re-engage attack)
                        bool moreEnemies = false;
                        for (int i = 0; i < 10 && _isScanning; i++) // 10 * 200ms = 2000ms (2s)
                        {
                            await Task.Delay(200);
                            if (_enemyScanner.CheckEnemyRedBarFromScreen(enemyX, enemyY, enemyW, enemyH))
                            {
                                moreEnemies = true;
                                info.EnemyHpPercentage = 50;
                                break;
                            }
                        }

                        if (!moreEnemies)
                        {
                            break;
                        }
                    }

                    // Looting sequence:
                    // After defeated enemy and waiting 2s safe, press C for looting.
                    _terminalLog.Log(6, "loot: pressing C");
                    _action.PressC(ignoreBlock: true);

                    Rect chestBounds = _overlay.GetSubRegionScreenBounds(6);
                    int cX = (int)Math.Round(chestBounds.X);
                    int cY = (int)Math.Round(chestBounds.Y);
                    int cW = (int)Math.Round(chestBounds.Width);
                    int cH = (int)Math.Round(chestBounds.Height);

                    bool chestOpened = false;

                    // Wait until chest returns actual value (up to 10s timeout, retrying C every 1.5s if character was walking)
                    DateTime lootStartTime = DateTime.UtcNow;
                    int cRetryCount = 0;

                    while (_isScanning && (DateTime.UtcNow - lootStartTime).TotalSeconds < 10)
                    {
                        await Task.Delay(250);

                        if (cW > 20 && cH > 20 && _chestScanner.IsChestOpen(cX, cY, cW, cH))
                        {
                            chestOpened = true;
                            _terminalLog.Log(6, "loot: chest opened");
                            break;
                        }

                        if ((DateTime.UtcNow - lootStartTime).TotalMilliseconds >= (cRetryCount + 1) * 1500 && cRetryCount < 3)
                        {
                            cRetryCount++;
                            _terminalLog.Log(6, $"loot: chest not yet opened, pressing C again (retry {cRetryCount})...");
                            _action.PressC(ignoreBlock: true);
                        }
                    }

                    if (chestOpened)
                    {
                        await ProcessChestLootingAsync(cX, cY, cW, cH);
                        await Task.Delay(400);

                        // Ensure chest is closed if still open
                        if (_chestScanner.IsChestOpen(cX, cY, cW, cH))
                        {
                            _terminalLog.Log(6, "loot: closing chest (press: X)");
                            _action.PressX();
                            await Task.Delay(400);
                        }
                    }
                    else
                    {
                        _terminalLog.Log(6, "loot: chest did not open (timeout)");
                    }

                    _isCombatActive = false; // Scan pipeline resumes now

                    // Then press P again to auto farm if was farming before enemy combat and AutoFarm is not blocked
                    if (wasFarming && !Engine.Services.Implements.Action.BlockAutoFarm)
                    {
                        await Task.Delay(250);
                        IsFarming = true;
                        _ = Task.Run(() => _action.AutoFarm());
                        _terminalLog.Log(2, $"farming: {IsFarming} | moving: false | resource found: true | action: farm (press: P)");
                    }

                    return true;
                }
                else
                {
                    if (hasEnergyNumber)
                    {
                        _terminalLog.Log(5, $"enemy: {info.HasEnemy} | energy: {energyDisplay} | blocked: energy number detected");
                    }
                    else
                    {
                        _terminalLog.Log(5, $"enemy: {info.HasEnemy} | energy: {energyDisplay}");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Enemy Pipeline Error]: {ex.Message}");
                _action.StopHoldingAttack();
                _isCombatActive = false;
                return false;
            }
            finally
            {
                _enemyLock.Release();
            }
        }
    }
}
