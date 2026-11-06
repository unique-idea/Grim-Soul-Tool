using System;

using System.Linq;

using System.Windows;

using System.Windows.Controls;

using System.Windows.Input;

using System.Windows.Media;

using UI.Models;

using UI.Services;



namespace UI.Windows

{

    public partial class SubRegionConfigWindow : Window

    {

        private readonly OverlayWindow _overlay;

        private int _selectedRegionId = 1;



        // Brushes

        private static readonly SolidColorBrush BrushNormalTabBg = new(Color.FromArgb(26, 255, 255, 255));

        private static readonly SolidColorBrush BrushActiveToggle = new(Color.FromArgb(70, 0, 230, 118));

        private static readonly SolidColorBrush BrushInactiveToggle = new(Color.FromArgb(30, 255, 255, 255));

        private static readonly SolidColorBrush BrushLockedToggle = new(Color.FromArgb(80, 255, 171, 0));



        /// <summary>

        /// True if user has manually moved this window, preventing toolbar auto-dock override.

        /// </summary>

        public bool IsCustomPositioned { get; set; } = false;



        public SubRegionConfigWindow(OverlayWindow overlay)

        {

            InitializeComponent();

            _overlay = overlay;



            _overlay.SubRegionsUpdated += (s, e) => Dispatcher.Invoke(RefreshUI);



            Loaded += (s, e) => SelectRegion(1);



            LocationChanged += (s, e) =>

            {

                if (IsLoaded && IsMouseOver)

                    IsCustomPositioned = true;

            };

        }



        private SubRegion? CurrentRegion => _overlay.SubRegions.FirstOrDefault(r => r.Id == _selectedRegionId);



        // ─── Drag to Move ─────────────────────────────────────────────────

        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {

            if (e.ButtonState == MouseButtonState.Pressed)

            {

                IsCustomPositioned = true;

                DragMove();

            }

        }



        // ─── Region Selection (Tabs) ──────────────────────────────────────

        private void BtnTabR1_Click(object sender, RoutedEventArgs e) => SelectRegion(1);

        private void BtnTabR2_Click(object sender, RoutedEventArgs e) => SelectRegion(2);

        private void BtnTabR3_Click(object sender, RoutedEventArgs e) => SelectRegion(3);

        private void BtnTabR4_Click(object sender, RoutedEventArgs e) => SelectRegion(4);

        private void BtnTabR5_Click(object sender, RoutedEventArgs e) => SelectRegion(5);

        private void BtnTabR6_Click(object sender, RoutedEventArgs e) => SelectRegion(6);



        private void SelectRegion(int id)

        {

            _selectedRegionId = id;



            // Highlight tabs

            BtnTabR1.Background = id == 1 ? new SolidColorBrush(Color.FromArgb(70, 0, 229, 255)) : BrushNormalTabBg;

            BtnTabR1.BorderBrush = id == 1 ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));



            BtnTabR2.Background = id == 2 ? new SolidColorBrush(Color.FromArgb(70, 255, 145, 0)) : BrushNormalTabBg;

            BtnTabR2.BorderBrush = id == 2 ? new SolidColorBrush(Color.FromRgb(255, 145, 0)) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));



            BtnTabR3.Background = id == 3 ? new SolidColorBrush(Color.FromArgb(70, 224, 64, 251)) : BrushNormalTabBg;

            BtnTabR3.BorderBrush = id == 3 ? new SolidColorBrush(Color.FromRgb(224, 64, 251)) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));



            BtnTabR4.Background = id == 4 ? new SolidColorBrush(Color.FromArgb(70, 255, 215, 0)) : BrushNormalTabBg;

            BtnTabR4.BorderBrush = id == 4 ? new SolidColorBrush(Color.FromRgb(255, 215, 0)) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));



            BtnTabR5.Background = id == 5 ? new SolidColorBrush(Color.FromArgb(70, 255, 23, 68)) : BrushNormalTabBg;

            BtnTabR5.BorderBrush = id == 5 ? new SolidColorBrush(Color.FromRgb(255, 23, 68)) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));



            BtnTabR6.Background = id == 6 ? new SolidColorBrush(Color.FromArgb(70, 0, 230, 118)) : BrushNormalTabBg;

            BtnTabR6.BorderBrush = id == 6 ? new SolidColorBrush(Color.FromRgb(0, 230, 118)) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));



            RefreshUI();

        }



        // ─── Refresh All Controls for Selected Region ─────────────────────

        public void RefreshUI()

        {

            var region = CurrentRegion;

            if (region == null) return;



            // Header card styling matching region theme

            RegionHeaderCard.BorderBrush = region.BorderBrush;

            RegionHeaderCard.Background = new SolidColorBrush(Color.FromArgb(25, region.ThemeColor.R, region.ThemeColor.G, region.ThemeColor.B));

            TxtRegionTitle.Text = region.Name;

            TxtRegionTitle.Foreground = region.BorderBrush;



            if (region.IsPinnedTopCenter)

            {

                TxtRegionSubtitle.Text = $"Top-Center Pinned · Size: {region.Width:0}×{region.Height:0}";

            }

            else if (region.Id == 1)

            {

                TxtRegionSubtitle.Text = $"Bar: {region.Width:0}×{region.Height:0} · Under (Text): {region.UnderWidth:0}×{region.UnderHeight:0}";

            }

            else

            {

                TxtRegionSubtitle.Text = $"Position: ({region.X:0}, {region.Y:0}) · Size: {region.Width:0}×{region.Height:0}";

            }



            // Lock badge

            if (region.IsPinnedTopCenter)

            {

                TxtLockBadge.Text = "📌 Top-Center";

                TxtLockBadge.Foreground = new SolidColorBrush(Color.FromRgb(255, 82, 82));

                BadgeLockStatus.Background = new SolidColorBrush(Color.FromArgb(60, 255, 23, 68));

            }

            else if (region.IsLocked)

            {

                TxtLockBadge.Text = "🔒 Locked to MR";

                TxtLockBadge.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 64));

                BadgeLockStatus.Background = new SolidColorBrush(Color.FromArgb(60, 255, 171, 0));

            }

            else

            {

                TxtLockBadge.Text = "🔓 Unlocked";

                TxtLockBadge.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));

                BadgeLockStatus.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));

            }



            // View toggle button

            TxtViewStatus.Text = region.IsVisible ? "Visible" : "Hidden";

            BtnToggleView.Background = region.IsVisible ? BrushActiveToggle : BrushInactiveToggle;



            // Grid toggle button & column

            if (region.HasGrid)

            {

                BtnToggleGrid.Visibility = Visibility.Visible;

                ColGridToggle.Width = new GridLength(1, GridUnitType.Star);

                TxtGridStatus.Text = region.IsGridVisible ? "Grid On" : "Grid Off";

                BtnToggleGrid.Background = region.IsGridVisible ? BrushActiveToggle : BrushInactiveToggle;

            }

            else

            {

                BtnToggleGrid.Visibility = Visibility.Collapsed;

                ColGridToggle.Width = new GridLength(0);

            }



            // Lock toggle button

            if (region.IsPinnedTopCenter)

            {

                BtnToggleLock.IsEnabled = false;

                TxtLockStatus.Text = "Pinned";

                TxtLockIcon.Text = "📌 ";

                BtnToggleLock.Background = BrushInactiveToggle;

            }

            else

            {

                BtnToggleLock.IsEnabled = true;

                TxtLockStatus.Text = region.IsLocked ? "Locked" : "Lock MR";

                TxtLockIcon.Text = region.IsLocked ? "🔒 " : "🔓 ";

                BtnToggleLock.Background = region.IsLocked ? BrushLockedToggle : BrushInactiveToggle;

            }



            // Scan Interval setting (hidden for on-demand event-triggered regions like R3 and R6)

            if (region.HasInterval)

            {

                IntervalConfigBorder.Visibility = Visibility.Visible;

                TxtInterval.Text = region.ScanIntervalMs.ToString();

            }

            else

            {

                IntervalConfigBorder.Visibility = Visibility.Collapsed;

            }



            // Heal Cooldown setting (only visible for Healthbar R1)

            if (region.Id == 1)

            {

                HealCooldownConfigBorder.Visibility = Visibility.Visible;

                TxtHealCd.Text = region.HealCooldownMs.ToString();

            }

            else

            {

                HealCooldownConfigBorder.Visibility = Visibility.Collapsed;

            }



            // Max HP setting (only visible for Healthbar R1)

            if (region.Id == 1)

            {

                MaxHpConfigBorder.Visibility = Visibility.Visible;

                TxtMaxHp.Text = region.MaxHealthValue.ToString();

            }

            else

            {

                MaxHpConfigBorder.Visibility = Visibility.Collapsed;

            }



            // Heal Threshold % setting (only visible for Healthbar R1)

            if (region.Id == 1)

            {

                HealThresholdConfigBorder.Visibility = Visibility.Visible;

                TxtHealThreshold.Text = region.HealThresholdPercent.ToString();

            }

            else

            {

                HealThresholdConfigBorder.Visibility = Visibility.Collapsed;

            }



            // Attack Threshold % setting (only visible for Enemy Bar R5)

            if (region.Id == 5)

            {

                EnemyThresholdConfigBorder.Visibility = Visibility.Visible;

                TxtEnemyThreshold.Text = region.EnemyThresholdPercent.ToString();



                MaxEnergyConfigBorder.Visibility = Visibility.Visible;

                TxtMaxEnergy.Text = region.MaxEnergyValue.ToString();

            }

            else

            {

                EnemyThresholdConfigBorder.Visibility = Visibility.Collapsed;

                MaxEnergyConfigBorder.Visibility = Visibility.Collapsed;

            }



            // Under Region setting (visible for Healthbar R1 and Enemy/Energy R5)

            if (region.Id == 1 || region.Id == 5)

            {

                HealthSplitConfigBorder.Visibility = Visibility.Visible;

                string label = region.Id == 1 ? "HP Text" : "Energy";

                TxtSplitSummary.Text = $"{label}: {region.UnderWidth:0}×{region.UnderHeight:0} (Offset: {region.UnderOffsetX:+0;-0;0}, {region.UnderOffsetY:+0;-0;0})";

                TxtSplitPercent.Text = "Attached";

            }

            else

            {

                HealthSplitConfigBorder.Visibility = Visibility.Collapsed;

            }



            // Grid row & col configuration

            if (region.Id == 2)

            {

                // Mini Map (R2) has linked square grid (10x10 -> 15x15 -> ... -> 30x30)

                MiniMapGridConfigBorder.Visibility = Visibility.Visible;

                GridConfigBorder.Visibility = Visibility.Collapsed;

                FixedNoticeBorder.Visibility = Visibility.Collapsed;



                TxtMiniMapGrid.Text = $"{region.Rows}×{region.Columns}";

                TxtMiniMapGridSummary.Text = $"{region.Rows}×{region.Columns}";

            }

            else if (region.HasGridSettings)

            {

                MiniMapGridConfigBorder.Visibility = Visibility.Collapsed;

                GridConfigBorder.Visibility = Visibility.Visible;

                FixedNoticeBorder.Visibility = Visibility.Collapsed;



                TxtRows.Text = region.Rows.ToString();

                TxtCols.Text = region.Columns.ToString();

            }

            else

            {

                MiniMapGridConfigBorder.Visibility = Visibility.Collapsed;

                GridConfigBorder.Visibility = Visibility.Collapsed;

                FixedNoticeBorder.Visibility = Visibility.Visible;



                if (region.Id == 1)

                    TxtFixedNotice.Text = $"Healthbar · Split: {region.HealthBarSplitPercent}% Bar (Top) / {100 - region.HealthBarSplitPercent}% Text (Bottom)";

                else if (region.Id == 4)

                    TxtFixedNotice.Text = "Equipment · Fixed 5 Rows × 1 Column";

                else if (region.Id == 5)

                    TxtFixedNotice.Text = "Enemy Bar · Pinned to Top-Center (No grid)";

            }



            // Center button: REMOVED for Enemy Bar (R5) as requested!

            if (region.IsPinnedTopCenter)

            {

                BtnCenterRegion.Visibility = Visibility.Collapsed;

                ColCenterBtn.Width = new GridLength(0);

                ColCenterGap.Width = new GridLength(0);

            }

            else

            {

                BtnCenterRegion.Visibility = Visibility.Visible;

                ColCenterBtn.Width = new GridLength(1, GridUnitType.Star);

                ColCenterGap.Width = new GridLength(6);

            }



            // Overview mini summaries

            UpdateOverview();

        }



        private void UpdateOverview()

        {

            foreach (var r in _overlay.SubRegions)

            {

                string status;

                if (r.Id == 1)

                {

                    status = $"⏱{r.ScanIntervalMs}ms · {(r.IsVisible ? "👁" : "🚫")} · Split {r.HealthBarSplitPercent}/{100 - r.HealthBarSplitPercent}%";

                }

                else if (r.IsPinnedTopCenter)

                {

                    status = $"⏱{r.ScanIntervalMs}ms · {(r.IsVisible ? "👁" : "🚫")} · 📌 Top";

                }

                else if (r.HasGrid)

                {

                    status = $"⏱{r.ScanIntervalMs}ms · {(r.IsVisible ? "👁" : "🚫")} · {(r.IsGridVisible ? "⊞" : "▢")} · {(r.IsLocked ? "🔒" : "🔓")}";

                }

                else

                {

                    status = $"⏱{r.ScanIntervalMs}ms · {(r.IsVisible ? "👁" : "🚫")} · {(r.IsLocked ? "🔒" : "🔓")}";

                }



                if (r.Id == 1) TxtOverviewR1.Text = status;

                else if (r.Id == 2) TxtOverviewR2.Text = status;

                else if (r.Id == 3) TxtOverviewR3.Text = status;

                else if (r.Id == 4) TxtOverviewR4.Text = status;

                else if (r.Id == 5) TxtOverviewR5.Text = status;

                else if (r.Id == 6) TxtOverviewR6.Text = status;

            }

        }



        // ─── Toggles ──────────────────────────────────────────────────────

        private void BtnToggleView_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.IsVisible = !region.IsVisible;

            _overlay.RedrawSubRegions();

            RefreshUI();

        }



        private void BtnToggleGrid_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || !region.HasGrid) return;

            region.IsGridVisible = !region.IsGridVisible;

            _overlay.RedrawSubRegions();

            RefreshUI();

        }



        private void BtnToggleLock_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || region.IsPinnedTopCenter) return;



            region.IsLocked = !region.IsLocked;



            if (region.IsLocked)

            {

                region.UpdateRelativePosition(_overlay.ActualWidth, _overlay.ActualHeight);

            }



            _overlay.RedrawSubRegions();

            RefreshUI();



            // Auto-save configuration whenever user locks or unlocks in Sub Task Bar

            ConfigService.Save(_overlay);

        }



        // ─── Scan Interval Configuration ─────────────────────────────────

        private void BtnIncInterval_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.ScanIntervalMs = Math.Min(60000, region.ScanIntervalMs + 250);

            TxtInterval.Text = region.ScanIntervalMs.ToString();

            UpdateOverview();

        }



        private void BtnDecInterval_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.ScanIntervalMs = Math.Max(50, region.ScanIntervalMs - 250);

            TxtInterval.Text = region.ScanIntervalMs.ToString();

            UpdateOverview();

        }



        private void TxtInterval_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedInterval();



        private void TxtInterval_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedInterval();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedInterval()

        {

            var region = CurrentRegion;

            if (region == null) return;



            if (int.TryParse(TxtInterval.Text.Trim(), out int val))

            {

                region.ScanIntervalMs = Math.Clamp(val, 50, 60000);

            }

            TxtInterval.Text = region.ScanIntervalMs.ToString();

            UpdateOverview();

        }



        // ─── Max HP Configuration (Healthbar R1) ──────────────────────────

        private void BtnIncMaxHp_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.MaxHealthValue = Math.Min(9999, region.MaxHealthValue + 5);

            TxtMaxHp.Text = region.MaxHealthValue.ToString();

            UpdateOverview();

        }



        private void BtnDecMaxHp_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.MaxHealthValue = Math.Max(1, region.MaxHealthValue - 5);

            TxtMaxHp.Text = region.MaxHealthValue.ToString();

            UpdateOverview();

        }



        private void TxtMaxHp_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedMaxHp();



        private void TxtMaxHp_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedMaxHp();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedMaxHp()

        {

            var region = CurrentRegion;

            if (region == null) return;

            if (int.TryParse(TxtMaxHp.Text.Trim(), out int val))

            {

                region.MaxHealthValue = Math.Clamp(val, 1, 9999);

            }

            TxtMaxHp.Text = region.MaxHealthValue.ToString();

            UpdateOverview();

        }



        // ─── Heal Cooldown Configuration (Healthbar R1) ───────────────────

        private void BtnIncHealCd_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.HealCooldownMs = Math.Min(30000, region.HealCooldownMs + 100);

            TxtHealCd.Text = region.HealCooldownMs.ToString();

            UpdateOverview();

        }



        private void BtnDecHealCd_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.HealCooldownMs = Math.Max(200, region.HealCooldownMs - 100);

            TxtHealCd.Text = region.HealCooldownMs.ToString();

            UpdateOverview();

        }



        private void TxtHealCd_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedHealCd();



        private void TxtHealCd_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedHealCd();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedHealCd()

        {

            var region = CurrentRegion;

            if (region == null) return;

            if (int.TryParse(TxtHealCd.Text.Trim(), out int val))

            {

                region.HealCooldownMs = Math.Clamp(val, 200, 30000);

            }

            TxtHealCd.Text = region.HealCooldownMs.ToString();

            UpdateOverview();

        }



        // ─── Heal Threshold (% HP) Configuration (Healthbar R1) ───────────

        private void BtnIncHealThreshold_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.HealThresholdPercent = Math.Min(95, region.HealThresholdPercent + 5);

            TxtHealThreshold.Text = region.HealThresholdPercent.ToString();

            UpdateOverview();

        }



        private void BtnDecHealThreshold_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.HealThresholdPercent = Math.Max(5, region.HealThresholdPercent - 5);

            TxtHealThreshold.Text = region.HealThresholdPercent.ToString();

            UpdateOverview();

        }



        private void TxtHealThreshold_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedHealThreshold();



        private void TxtHealThreshold_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedHealThreshold();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedHealThreshold()

        {

            var region = CurrentRegion;

            if (region == null) return;

            if (int.TryParse(TxtHealThreshold.Text.Trim(), out int val))

            {

                region.HealThresholdPercent = Math.Clamp(val, 5, 95);

            }

            TxtHealThreshold.Text = region.HealThresholdPercent.ToString();

            UpdateOverview();

        }



        // ─── Enemy Attack Threshold (% Width) Configuration (Enemy Bar R5) ──

        private void BtnIncEnemyThreshold_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.EnemyThresholdPercent = Math.Min(95, region.EnemyThresholdPercent + 5);

            TxtEnemyThreshold.Text = region.EnemyThresholdPercent.ToString();

            UpdateOverview();

        }



        private void BtnDecEnemyThreshold_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.EnemyThresholdPercent = Math.Max(10, region.EnemyThresholdPercent - 5);

            TxtEnemyThreshold.Text = region.EnemyThresholdPercent.ToString();

            UpdateOverview();

        }



        private void TxtEnemyThreshold_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedEnemyThreshold();



        private void TxtEnemyThreshold_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedEnemyThreshold();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedEnemyThreshold()

        {

            var region = CurrentRegion;

            if (region == null) return;

            if (int.TryParse(TxtEnemyThreshold.Text.Trim(), out int val))

            {

                region.EnemyThresholdPercent = Math.Clamp(val, 10, 95);

            }

            TxtEnemyThreshold.Text = region.EnemyThresholdPercent.ToString();

            UpdateOverview();

        }



        // ─── Max Energy Configuration (Enemy Bar R5) ─────────────────────

        private void BtnIncMaxEnergy_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.MaxEnergyValue = Math.Min(9999, region.MaxEnergyValue + 10);

            TxtMaxEnergy.Text = region.MaxEnergyValue.ToString();

            UpdateOverview();

        }



        private void BtnDecMaxEnergy_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;

            region.MaxEnergyValue = Math.Max(1, region.MaxEnergyValue - 10);

            TxtMaxEnergy.Text = region.MaxEnergyValue.ToString();

            UpdateOverview();

        }



        private void TxtMaxEnergy_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedMaxEnergy();



        private void TxtMaxEnergy_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedMaxEnergy();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedMaxEnergy()

        {

            var region = CurrentRegion;

            if (region == null) return;

            if (int.TryParse(TxtMaxEnergy.Text.Trim(), out int val))

            {

                region.MaxEnergyValue = Math.Clamp(val, 1, 9999);

            }

            TxtMaxEnergy.Text = region.MaxEnergyValue.ToString();

            UpdateOverview();

        }



        // ─── Mini Map Linked Grid Size Configuration (10x10 -> 15x15 -> ... -> 30x30) ───

        private static readonly int[] MiniMapGridSteps = { 10, 15, 20, 25, 30 };



        private void BtnIncMiniMapGrid_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || region.Id != 2) return;



            int currentN = Math.Max(region.Rows, region.Columns);

            int nextN = 30;

            foreach (int step in MiniMapGridSteps)

            {

                if (step > currentN)

                {

                    nextN = step;

                    break;

                }

            }



            region.Rows = nextN;

            region.Columns = nextN;

            TxtMiniMapGrid.Text = $"{nextN}×{nextN}";

            TxtMiniMapGridSummary.Text = $"{nextN}×{nextN}";

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        private void BtnDecMiniMapGrid_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || region.Id != 2) return;



            int currentN = Math.Min(region.Rows, region.Columns);

            int prevN = 10;

            for (int i = MiniMapGridSteps.Length - 1; i >= 0; i--)

            {

                if (MiniMapGridSteps[i] < currentN)

                {

                    prevN = MiniMapGridSteps[i];

                    break;

                }

            }



            region.Rows = prevN;

            region.Columns = prevN;

            TxtMiniMapGrid.Text = $"{prevN}×{prevN}";

            TxtMiniMapGridSummary.Text = $"{prevN}×{prevN}";

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        private void TxtMiniMapGrid_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedMiniMapGrid();



        private void TxtMiniMapGrid_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedMiniMapGrid();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedMiniMapGrid()

        {

            var region = CurrentRegion;

            if (region == null || region.Id != 2) return;



            string text = TxtMiniMapGrid.Text.Trim().ToLower().Replace("x", "×").Replace(" ", "");

            var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");

            if (match.Success && int.TryParse(match.Value, out int val))

            {

                int clamped = Math.Clamp(val, 10, 30);

                region.Rows = clamped;

                region.Columns = clamped;

            }



            TxtMiniMapGrid.Text = $"{region.Rows}×{region.Columns}";

            TxtMiniMapGridSummary.Text = $"{region.Rows}×{region.Columns}";

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        // ─── Row Increment / Decrement & Input ────────────────────────────

        private void BtnIncRow_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || !region.HasGridSettings) return;

            region.Rows = Math.Min(50, region.Rows + 1);

            TxtRows.Text = region.Rows.ToString();

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        private void BtnDecRow_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || !region.HasGridSettings) return;

            region.Rows = Math.Max(1, region.Rows - 1);

            TxtRows.Text = region.Rows.ToString();

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        private void TxtRows_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedRows();



        private void TxtRows_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedRows();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedRows()

        {

            var region = CurrentRegion;

            if (region == null || !region.HasGridSettings) return;



            if (int.TryParse(TxtRows.Text.Trim(), out int val))

            {

                region.Rows = Math.Clamp(val, 1, 50);

            }

            TxtRows.Text = region.Rows.ToString();

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        // ─── Column Increment / Decrement & Input ─────────────────────────

        private void BtnIncCol_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || !region.HasGridSettings) return;

            region.Columns = Math.Min(50, region.Columns + 1);

            TxtCols.Text = region.Columns.ToString();

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        private void BtnDecCol_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || !region.HasGridSettings) return;

            region.Columns = Math.Max(1, region.Columns - 1);

            TxtCols.Text = region.Columns.ToString();

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        private void TxtCols_LostFocus(object sender, RoutedEventArgs e) => ApplyTypedCols();



        private void TxtCols_KeyDown(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                ApplyTypedCols();

                Keyboard.ClearFocus();

            }

        }



        private void ApplyTypedCols()

        {

            var region = CurrentRegion;

            if (region == null || !region.HasGridSettings) return;



            if (int.TryParse(TxtCols.Text.Trim(), out int val))

            {

                region.Columns = Math.Clamp(val, 1, 50);

            }

            TxtCols.Text = region.Columns.ToString();

            _overlay.RedrawSubRegions();

            UpdateOverview();

        }



        // ─── Quick Alignment Tools ────────────────────────────────────────

        private void BtnCenterRegion_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null || region.IsPinnedTopCenter) return;



            region.X = Math.Max(0, (_overlay.ActualWidth - region.Width) / 2);

            region.Y = Math.Max(0, (_overlay.ActualHeight - region.Height) / 2);

            region.UpdateRelativePosition(_overlay.ActualWidth, _overlay.ActualHeight);



            _overlay.RedrawSubRegions();

            RefreshUI();

        }



        private void BtnResetPreset_Click(object sender, RoutedEventArgs e)

        {

            var region = CurrentRegion;

            if (region == null) return;



            if (region.Id == 1)

            {

                region.Width = 150; region.Height = 40; region.Rows = 1; region.Columns = 1;

            }

            else if (region.Id == 2)

            {

                region.Width = 130; region.Height = 110; region.Rows = 3; region.Columns = 3;

            }

            else if (region.Id == 3)

            {

                region.Width = 175; region.Height = 140; region.Rows = 4; region.Columns = 5;

            }

            else if (region.Id == 4)

            {

                region.Width = 52; region.Height = 210; region.Rows = 5; region.Columns = 1;

            }

            else if (region.Id == 5)

            {

                region.Width = 200; region.Height = 36; region.Rows = 1; region.Columns = 1;

                region.X = Math.Max(0, (_overlay.ActualWidth - region.Width) / 2);

                region.Y = 8;

            }



            region.UpdateRelativePosition(_overlay.ActualWidth, _overlay.ActualHeight);

            _overlay.RedrawSubRegions();

            RefreshUI();

        }

    }

}

