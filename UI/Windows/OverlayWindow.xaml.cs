using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using UI.Models;

namespace UI.Windows
{
    /// <summary>
    /// Transparent, always-on-top overlay rectangle (Main Rectangle - MR).
    /// Moved via the bottom-right green handle [✥ MR].
    /// Contains 5 configurable sub-regions for focused scanning:
    /// R1: Healthbar (no grid)
    /// R2: Mini Map (flexible grid)
    /// R3: Inventory (flexible grid)
    /// R4: Equipment (fixed 5 rows × 1 col)
    /// R5: Enemy Bar (no grid, pinned to top-center of MR)
    /// </summary>
    public partial class OverlayWindow : Window
    {
        // ─── Win32 API ───────────────────────────────────────────────────────
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            return new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int HTTRANSPARENT = -1;

        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        // ─── MR Grid state ───────────────────────────────────────────────────
        private bool _gridVisible = false;
        private int _gridN = 10;
        private int _centerIgnoredRadius = 1;

        private static readonly Color CellColorA = Color.FromArgb(8, 0, 230, 118);
        private static readonly Color CellColorB = Color.FromArgb(8, 64, 196, 255);
        private static readonly Color CenterCellColor = Color.FromArgb(70, 255, 23, 68);
        private static readonly Color GridLineColor = Color.FromArgb(45, 0, 230, 118);

        // ─── Sub-Regions (5 scan zones) ──────────────────────────────────────
        private readonly List<SubRegion> _subRegions = new();
        public IReadOnlyList<SubRegion> SubRegions => _subRegions;

        public event EventHandler? SubRegionsUpdated;

        /// <summary>
        /// True if custom positions/sizes have been applied from config.json.
        /// </summary>
        public bool HasLoadedConfig { get; set; } = false;

        public OverlayWindow()
        {
            InitializeComponent();

            InitializeSubRegions();

            Loaded += (s, e) =>
            {
                foreach (var r in _subRegions)
                {
                    if (r.IsPinnedTopCenter && !HasLoadedConfig)
                    {
                        r.X = Math.Max(0, (ActualWidth - r.Width) / 2);
                        r.Y = 8;
                    }
                    r.UpdateRelativePosition(ActualWidth, ActualHeight);
                }
                RedrawSubRegions();
            };
        }

        private void InitializeSubRegions()
        {
            // R1: Healthbar (Top: visual health bar width, Under: exact current HP text box, attached to each other)
            var r1 = new SubRegion(1, "Healthbar", Color.FromRgb(0, 229, 255), 20, 20, 140, 16, rows: 1, columns: 1)
            {
                HasGrid = false,
                HasGridSettings = false,
                ScanIntervalMs = 500,
                UnderWidth = 65,
                UnderHeight = 22,
                UnderOffsetX = 0,
                UnderOffsetY = 4
            };

            // R2: Mini Map (flexible grid, 1000ms radar interval, default 10x10)
            var r2 = new SubRegion(2, "Mini Map", Color.FromRgb(255, 145, 0), 450, 20, 130, 110, rows: 10, columns: 10)
            {
                HasGrid = true,
                HasGridSettings = true,
                ScanIntervalMs = 1000
            };

            // R3: Inventory (fixed 5 rows x 5 columns, scanned on-demand/event without interval loop)
            var r3 = new SubRegion(3, "Inventory", Color.FromRgb(224, 64, 251), 420, 250, 175, 175, rows: 5, columns: 5)
            {
                HasGrid = true,
                HasGridSettings = false,
                HasInterval = false
            };

            // R4: Equipment (fixed 5 rows x 1 column, 3000ms interval)
            var r4 = new SubRegion(4, "Equipment", Color.FromRgb(255, 215, 0), 20, 180, 52, 210, rows: 5, columns: 1)
            {
                HasGrid = true,
                HasGridSettings = false,
                ScanIntervalMs = 3000
            };

            // R5: Enemy Bar (no grid, locked in middle of top MR, 1500ms default interval, attached Energy region)
            var r5 = new SubRegion(5, "Enemy Bar", Color.FromRgb(255, 23, 68), 220, 8, 200, 36, rows: 1, columns: 1)
            {
                HasGrid = false,
                HasGridSettings = false,
                IsPinnedTopCenter = true,
                ScanIntervalMs = 1500,
                MaxEnergyValue = 100,
                UnderWidth = 65,
                UnderHeight = 22,
                UnderOffsetX = 0,
                UnderOffsetY = 4
            };

            // R6: Chest / Loot container (fixed 5 rows x 5 columns, scanned on-demand/event without interval loop)
            var r6 = new SubRegion(6, "Chest", Color.FromRgb(0, 230, 118), 650, 250, 175, 175, rows: 5, columns: 5)
            {
                HasGrid = true,
                HasGridSettings = false,
                HasInterval = false
            };

            _subRegions.Add(r1);
            _subRegions.Add(r2);
            _subRegions.Add(r3);
            _subRegions.Add(r4);
            _subRegions.Add(r5);
            _subRegions.Add(r6);

            foreach (var r in _subRegions)
            {
                r.Changed += (s, e) =>
                {
                    RedrawSubRegions();
                    SubRegionsUpdated?.Invoke(this, EventArgs.Empty);
                };
                r.PositionChanged += (s, e) =>
                {
                    SubRegionsUpdated?.Invoke(this, EventArgs.Empty);
                };
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = (HwndSource?)PresentationSource.FromVisual(this);
            source?.AddHook(WndProc);
        }

        // â”€â”€â”€ Click-Through (Play Mode) Toggle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public bool IsClickThrough { get; private set; } = false;

        public void SetClickThrough(bool enable)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetClickThrough(enable));
                return;
            }

            IsClickThrough = enable;

            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                try
                {
                    long exStyle = GetWindowLong(hwnd, GWL_EXSTYLE).ToInt64();
                    if (enable)
                    {
                        exStyle |= (WS_EX_TRANSPARENT | WS_EX_LAYERED);
                    }
                    else
                    {
                        exStyle &= ~WS_EX_TRANSPARENT;
                    }
                    SetWindowLong(hwnd, GWL_EXSTYLE, new IntPtr(exStyle));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Overlay SetClickThrough Error]: {ex.Message}");
                }
            }

            if (RootGrid != null) RootGrid.IsHitTestVisible = !enable;
            if (MrMoveHandle != null) MrMoveHandle.Visibility = enable ? Visibility.Collapsed : Visibility.Visible;
            if (BorderRect != null)
            {
                BorderRect.StrokeDashArray = enable ? new DoubleCollection { 4, 6 } : new DoubleCollection { 8, 4 };
                BorderRect.Opacity = enable ? 0.6 : 1.0;
            }
        }

        // â”€â”€â”€ Custom Hit-Testing â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // CRITICAL: Only process WM_NCHITTEST. All other Windows messages must be passed through to WPF!
            if (msg != WM_NCHITTEST) return IntPtr.Zero;

            if (IsClickThrough)
            {
                handled = true;
                return new IntPtr(HTTRANSPARENT);
            }

            GetWindowRect(hwnd, out RECT rect);

            int screenX = unchecked((short)(lParam.ToInt32() & 0xFFFF));
            int screenY = unchecked((short)((lParam.ToInt32() >> 16) & 0xFFFF));

            int relX = screenX - rect.Left;
            int relY = screenY - rect.Top;
            int w = rect.Right - rect.Left;
            int h = rect.Bottom - rect.Top;

            const int EDGE = 7;
            const int CORNER = 16;

            bool nearLeft = relX < EDGE;
            bool nearRight = relX > w - EDGE;
            bool nearTop = relY < EDGE;
            bool nearBottom = relY > h - EDGE;

            bool inCornerLeft = relX < CORNER;
            bool inCornerRight = relX > w - CORNER;

            handled = true;

            // Outer corners — MR resize
            if (nearTop && inCornerLeft) return new IntPtr(HTTOPLEFT);
            if (nearTop && inCornerRight) return new IntPtr(HTTOPRIGHT);
            if (nearBottom && inCornerLeft) return new IntPtr(HTBOTTOMLEFT);
            if (nearBottom && inCornerRight) return new IntPtr(HTBOTTOMRIGHT);

            // Outer edges — MR resize
            if (nearTop) return new IntPtr(HTTOP);
            if (nearBottom) return new IntPtr(HTBOTTOM);
            if (nearLeft) return new IntPtr(HTLEFT);
            if (nearRight) return new IntPtr(HTRIGHT);

            // Interior is HTCLIENT so all sub-regions and MR move handle receive WPF mouse events
            return new IntPtr(HTCLIENT);
        }

        // ─── MR Move Handle (Bottom-Right Corner) ───────────────────────────
        private void MrMoveHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // ─── Public API ──────────────────────────────────────────────────────
        public bool IsGridVisible => _gridVisible;
        public int GridN => _gridN;
        public int CenterIgnoredRadius => _centerIgnoredRadius;

        public Rect GetScreenBounds()
        {
            Point topLeft = PointToScreen(new Point(0, 0));
            Point bottomRight = PointToScreen(new Point(ActualWidth, ActualHeight));
            return new Rect(topLeft.X, topLeft.Y, Math.Max(1, bottomRight.X - topLeft.X), Math.Max(1, bottomRight.Y - topLeft.Y));
        }

        public Rect GetSubRegionScreenBounds(int id)
        {
            var region = _subRegions.FirstOrDefault(r => r.Id == id);
            if (region == null || !region.IsVisible) return Rect.Empty;

            // PointToScreen maps WPF local coordinates directly to physical screen pixels with 100% DPI accuracy
            Point topLeft = PointToScreen(new Point(region.X, region.Y));
            Point bottomRight = PointToScreen(new Point(region.X + region.Width, region.Y + region.Height));

            double physicalWidth = Math.Max(1, bottomRight.X - topLeft.X);
            double physicalHeight = Math.Max(1, bottomRight.Y - topLeft.Y);

            return new Rect(topLeft.X, topLeft.Y, physicalWidth, physicalHeight);
        }

        public Rect GetHealthBarTextScreenBounds()
        {
            var r1 = _subRegions.FirstOrDefault(r => r.Id == 1);
            if (r1 == null || !r1.IsVisible) return Rect.Empty;

            double textLeft = r1.X + r1.UnderOffsetX;
            double textTop = r1.Y + r1.Height + r1.UnderOffsetY;
            double textW = r1.UnderWidth;
            double textH = r1.UnderHeight;

            Point topLeft = PointToScreen(new Point(textLeft, textTop));
            Point bottomRight = PointToScreen(new Point(textLeft + textW, textTop + textH));

            double physicalWidth = Math.Max(1, bottomRight.X - topLeft.X);
            double physicalHeight = Math.Max(1, bottomRight.Y - topLeft.Y);

            return new Rect(topLeft.X, topLeft.Y, physicalWidth, physicalHeight);
        }

        public Rect GetEnemyEnergyTextScreenBounds()
        {
            var r5 = _subRegions.FirstOrDefault(r => r.Id == 5);
            if (r5 == null || !r5.IsVisible) return Rect.Empty;

            double energyLeft = r5.X + r5.UnderOffsetX;
            double energyTop = r5.Y + r5.Height + r5.UnderOffsetY;
            double energyW = r5.UnderWidth;
            double energyH = r5.UnderHeight;

            Point topLeft = PointToScreen(new Point(energyLeft, energyTop));
            Point bottomRight = PointToScreen(new Point(energyLeft + energyW, energyTop + energyH));

            double physicalWidth = Math.Max(1, bottomRight.X - topLeft.X);
            double physicalHeight = Math.Max(1, bottomRight.Y - topLeft.Y);

            return new Rect(topLeft.X, topLeft.Y, physicalWidth, physicalHeight);
        }

        public void ToggleGrid()
        {
            _gridVisible = !_gridVisible;
            GridCanvas.Visibility = _gridVisible ? Visibility.Visible : Visibility.Collapsed;
            if (_gridVisible) DrawGrid();
        }

        public void SetGridSize(int n, int centerIgnoredRadius)
        {
            _gridN = n;
            _centerIgnoredRadius = centerIgnoredRadius;
            if (_gridVisible) DrawGrid();
        }

        // ─── MR Grid Drawing ─────────────────────────────────────────────────
        private void DrawGrid()
        {
            GridCanvas.Children.Clear();

            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            double cellW = w / _gridN;
            double cellH = h / _gridN;

            int centerBase = _gridN / 2;
            int minCenterRow = centerBase - CenterIgnoredRadius;
            int maxCenterRow = centerBase + CenterIgnoredRadius - 1;
            int minCenterCol = centerBase - CenterIgnoredRadius;
            int maxCenterCol = centerBase + CenterIgnoredRadius - 1;

            for (int row = 0; row < _gridN; row++)
            {
                for (int col = 0; col < _gridN; col++)
                {
                    bool isCenterZone = (row >= minCenterRow && row <= maxCenterRow) &&
                                        (col >= minCenterCol && col <= maxCenterCol);

                    Brush fillBrush = isCenterZone
                        ? new SolidColorBrush(CenterCellColor)
                        : new SolidColorBrush((row + col) % 2 == 0 ? CellColorA : CellColorB);

                    var cell = new Rectangle
                    {
                        Width = cellW,
                        Height = cellH,
                        Fill = fillBrush
                    };

                    Canvas.SetLeft(cell, col * cellW);
                    Canvas.SetTop(cell, row * cellH);
                    GridCanvas.Children.Add(cell);
                }
            }

            var lineBrush = new SolidColorBrush(GridLineColor);
            for (int col = 1; col < _gridN; col++)
            {
                double x = col * cellW;
                GridCanvas.Children.Add(new Line
                {
                    X1 = x, Y1 = 0, X2 = x, Y2 = h,
                    Stroke = lineBrush,
                    StrokeThickness = 0.6
                });
            }

            for (int row = 1; row < _gridN; row++)
            {
                double y = row * cellH;
                GridCanvas.Children.Add(new Line
                {
                    X1 = 0, Y1 = y, X2 = w, Y2 = y,
                    Stroke = lineBrush,
                    StrokeThickness = 0.6
                });
            }
        }

        // ─── Sub-Regions Rendering & Interaction ─────────────────────────────
        public void RedrawSubRegions()
        {
            SubRegionsCanvas.Children.Clear();

            foreach (var region in _subRegions)
            {
                if (!region.IsVisible) continue;

                if (region.Id == 1)
                {
                    RenderAttachedHealthBarRegions(SubRegionsCanvas, region);
                }
                else if (region.Id == 5)
                {
                    RenderAttachedEnemyEnergyRegions(SubRegionsCanvas, region);
                }
                else
                {
                    var visual = CreateSubRegionVisual(region);
                    SubRegionsCanvas.Children.Add(visual);
                }
            }
        }

        private void RenderAttachedHealthBarRegions(Canvas canvas, SubRegion region)
        {
            // ── Top Box: Health Bar Width Region ──────────────────────
            var barContainer = new Grid
            {
                Width = region.Width,
                Height = region.Height,
                Cursor = region.IsLocked ? Cursors.Arrow : Cursors.SizeAll,
                SnapsToDevicePixels = true
            };
            Canvas.SetLeft(barContainer, region.X);
            Canvas.SetTop(barContainer, region.Y);

            var barBorder = new Border
            {
                BorderBrush = region.BorderBrush,
                BorderThickness = new Thickness(region.IsLocked ? 2.5 : 1.5),
                Background = region.FillBrush,
                CornerRadius = new CornerRadius(4),
                IsHitTestVisible = true
            };
            barContainer.Children.Add(barBorder);

            // Badge above the bar
            var barBadge = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromArgb(220, 8, 14, 26)),
                BorderBrush = region.BorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, -18, 0, 0),
                Padding = new Thickness(5, 1, 5, 1),
                IsHitTestVisible = false
            };
            barBadge.Child = new TextBlock
            {
                Text = "R1: Health Bar (Width)",
                FontSize = 9.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            barContainer.Children.Add(barBadge);

            // ── Under Box: Exact Current HP Text Box (Attached) ───────
            double underLeft = region.X + region.UnderOffsetX;
            double underTop = region.Y + region.Height + region.UnderOffsetY;

            var textContainer = new Grid
            {
                Width = region.UnderWidth,
                Height = region.UnderHeight,
                Cursor = region.IsLocked ? Cursors.Arrow : Cursors.SizeAll,
                SnapsToDevicePixels = true
            };
            Canvas.SetLeft(textContainer, underLeft);
            Canvas.SetTop(textContainer, underTop);

            var textBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 230, 118)), // Bright Green Accent
                BorderThickness = new Thickness(1.5),
                Background = new SolidColorBrush(Color.FromArgb(50, 0, 230, 118)),
                CornerRadius = new CornerRadius(4),
                IsHitTestVisible = true
            };
            textContainer.Children.Add(textBorder);

            // Badge for text box
            var textBadge = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromArgb(220, 8, 14, 26)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, -16, 0, 0),
                Padding = new Thickness(4, 1, 4, 1),
                IsHitTestVisible = false
            };
            textBadge.Child = new TextBlock
            {
                Text = "🔢 Current HP (Text)",
                FontSize = 8.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                VerticalAlignment = VerticalAlignment.Center
            };
            textContainer.Children.Add(textBadge);

            // Connector link (visual dashed line showing they are attached)
            var connectorLine = new Line
            {
                X1 = region.X + region.UnderOffsetX + 12,
                Y1 = region.Y + region.Height,
                X2 = region.X + region.UnderOffsetX + 12,
                Y2 = underTop,
                Stroke = new SolidColorBrush(Color.FromArgb(180, 0, 229, 255)),
                StrokeThickness = 1.2,
                StrokeDashArray = new DoubleCollection { 2, 2 },
                IsHitTestVisible = false
            };

            void UpdatePositions(double newBarX, double newBarY, double newBarH)
            {
                Canvas.SetLeft(barContainer, newBarX);
                Canvas.SetTop(barContainer, newBarY);

                double uL = newBarX + region.UnderOffsetX;
                double uT = newBarY + newBarH + region.UnderOffsetY;
                Canvas.SetLeft(textContainer, uL);
                Canvas.SetTop(textContainer, uT);

                connectorLine.X1 = uL + 12;
                connectorLine.Y1 = newBarY + newBarH;
                connectorLine.X2 = uL + 12;
                connectorLine.Y2 = uT;
            }

            if (!region.IsLocked)
            {
                // Resize grip for Bar (Bottom-Right corner)
                var barResizeGrip = new Border
                {
                    Width = 14,
                    Height = 14,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = region.BorderBrush,
                    Opacity = 0.9,
                    Cursor = Cursors.SizeNWSE,
                    CornerRadius = new CornerRadius(3, 0, 3, 0),
                    ToolTip = "Drag to resize Health Bar width/height"
                };
                barContainer.Children.Add(barResizeGrip);

                bool isBarResizing = false;
                Point startBarResizeMouse = default;
                double startBarW = 0, startBarH = 0;

                barResizeGrip.MouseLeftButtonDown += (s, e) =>
                {
                    isBarResizing = true;
                    startBarResizeMouse = e.GetPosition(this);
                    startBarW = region.Width;
                    startBarH = region.Height;
                    barResizeGrip.CaptureMouse();
                    e.Handled = true;
                };
                barResizeGrip.MouseMove += (s, e) =>
                {
                    if (!isBarResizing) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startBarResizeMouse.X;
                    double dY = cur.Y - startBarResizeMouse.Y;

                    double maxW = Math.Max(35, ActualWidth - region.X);
                    double maxH = Math.Max(8, ActualHeight - region.Y);

                    double newW = Math.Clamp(startBarW + dX, 35, maxW);
                    double newH = Math.Clamp(startBarH + dY, 8, maxH);

                    barContainer.Width = newW;
                    barContainer.Height = newH;
                    region.SetSizeSilently(newW, newH);
                    UpdatePositions(region.X, region.Y, newH);
                    e.Handled = true;
                };
                barResizeGrip.MouseLeftButtonUp += (s, e) =>
                {
                    if (isBarResizing)
                    {
                        isBarResizing = false;
                        barResizeGrip.ReleaseMouseCapture();
                        region.UpdateRelativePosition(ActualWidth, ActualHeight);
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };

                // Drag move for Bar: moves BOTH attached regions together!
                bool isBarDragging = false;
                Point startBarDragMouse = default;
                double startBarX = 0, startBarY = 0;

                barContainer.MouseLeftButtonDown += (s, e) =>
                {
                    isBarDragging = true;
                    startBarDragMouse = e.GetPosition(this);
                    startBarX = region.X;
                    startBarY = region.Y;
                    barContainer.CaptureMouse();
                    e.Handled = true;
                };
                barContainer.MouseMove += (s, e) =>
                {
                    if (!isBarDragging) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startBarDragMouse.X;
                    double dY = cur.Y - startBarDragMouse.Y;

                    double maxW = Math.Max(0, ActualWidth - region.Width);
                    double maxH = Math.Max(0, ActualHeight - region.Height);

                    double newX = Math.Clamp(startBarX + dX, 0, maxW);
                    double newY = Math.Clamp(startBarY + dY, 0, maxH);

                    region.SetPositionSilently(newX, newY);
                    UpdatePositions(newX, newY, region.Height);
                    e.Handled = true;
                };
                barContainer.MouseLeftButtonUp += (s, e) =>
                {
                    if (isBarDragging)
                    {
                        isBarDragging = false;
                        barContainer.ReleaseMouseCapture();
                        region.UpdateRelativePosition(ActualWidth, ActualHeight);
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };

                // Resize grip for Under Box (Current HP Text)
                var textResizeGrip = new Border
                {
                    Width = 14,
                    Height = 14,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = new SolidColorBrush(Color.FromRgb(0, 230, 118)),
                    Opacity = 0.9,
                    Cursor = Cursors.SizeNWSE,
                    CornerRadius = new CornerRadius(3, 0, 3, 0),
                    ToolTip = "Drag to resize Under Region to cover exact current HP number"
                };
                textContainer.Children.Add(textResizeGrip);

                bool isTextResizing = false;
                Point startTextResizeMouse = default;
                double startTextW = 0, startTextH = 0;

                textResizeGrip.MouseLeftButtonDown += (s, e) =>
                {
                    isTextResizing = true;
                    startTextResizeMouse = e.GetPosition(this);
                    startTextW = region.UnderWidth;
                    startTextH = region.UnderHeight;
                    textResizeGrip.CaptureMouse();
                    e.Handled = true;
                };
                textResizeGrip.MouseMove += (s, e) =>
                {
                    if (!isTextResizing) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startTextResizeMouse.X;
                    double dY = cur.Y - startTextResizeMouse.Y;

                    double newW = Math.Clamp(startTextW + dX, 20, 400);
                    double newH = Math.Clamp(startTextH + dY, 10, 150);

                    textContainer.Width = newW;
                    textContainer.Height = newH;
                    region.SetUnderSizeSilently(newW, newH);
                    e.Handled = true;
                };
                textResizeGrip.MouseLeftButtonUp += (s, e) =>
                {
                    if (isTextResizing)
                    {
                        isTextResizing = false;
                        textResizeGrip.ReleaseMouseCapture();
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };

                // Drag move for Under Box: adjusts offset so user can position it exactly over the number!
                bool isTextDragging = false;
                Point startTextDragMouse = default;
                double startOffsetX = 0, startOffsetY = 0;

                textContainer.MouseLeftButtonDown += (s, e) =>
                {
                    isTextDragging = true;
                    startTextDragMouse = e.GetPosition(this);
                    startOffsetX = region.UnderOffsetX;
                    startOffsetY = region.UnderOffsetY;
                    textContainer.CaptureMouse();
                    e.Handled = true;
                };
                textContainer.MouseMove += (s, e) =>
                {
                    if (!isTextDragging) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startTextDragMouse.X;
                    double dY = cur.Y - startTextDragMouse.Y;

                    double newOffsetX = Math.Clamp(startOffsetX + dX, -300, 500);
                    double newOffsetY = Math.Clamp(startOffsetY + dY, -50, 300);

                    region.SetUnderOffsetSilently(newOffsetX, newOffsetY);

                    double uL = region.X + newOffsetX;
                    double uT = region.Y + region.Height + newOffsetY;
                    Canvas.SetLeft(textContainer, uL);
                    Canvas.SetTop(textContainer, uT);

                    connectorLine.X1 = uL + 12;
                    connectorLine.Y1 = region.Y + region.Height;
                    connectorLine.X2 = uL + 12;
                    connectorLine.Y2 = uT;

                    e.Handled = true;
                };
                textContainer.MouseLeftButtonUp += (s, e) =>
                {
                    if (isTextDragging)
                    {
                        isTextDragging = false;
                        textContainer.ReleaseMouseCapture();
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };
            }

            canvas.Children.Add(connectorLine);
            canvas.Children.Add(barContainer);
            canvas.Children.Add(textContainer);
        }

        private void RenderAttachedEnemyEnergyRegions(Canvas canvas, SubRegion region)
        {
            // ── Top Box: Enemy Health Bar (Red Line) ───────────────────
            var enemyContainer = new Grid
            {
                Width = region.Width,
                Height = region.Height,
                Cursor = region.IsLocked ? Cursors.Arrow : Cursors.SizeAll,
                SnapsToDevicePixels = true
            };
            Canvas.SetLeft(enemyContainer, region.X);
            Canvas.SetTop(enemyContainer, region.Y);

            var enemyBorder = new Border
            {
                BorderBrush = region.BorderBrush,
                BorderThickness = new Thickness(region.IsLocked ? 2.5 : 1.5),
                Background = region.FillBrush,
                CornerRadius = new CornerRadius(4),
                IsHitTestVisible = true
            };
            enemyContainer.Children.Add(enemyBorder);

            // Badge for Enemy Bar
            var enemyBadge = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromArgb(220, 8, 14, 26)),
                BorderBrush = region.BorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, -18, 0, 0),
                Padding = new Thickness(5, 1, 5, 1),
                IsHitTestVisible = false
            };
            enemyBadge.Child = new TextBlock
            {
                Text = "R5: Enemy Bar (Red Line)",
                FontSize = 9.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            enemyContainer.Children.Add(enemyBadge);

            // ── Under Box: Attached Energy Box (Text OCR) ─────────────
            double underLeft = region.X + region.UnderOffsetX;
            double underTop = region.Y + region.Height + region.UnderOffsetY;

            var energyContainer = new Grid
            {
                Width = region.UnderWidth,
                Height = region.UnderHeight,
                Cursor = region.IsLocked ? Cursors.Arrow : Cursors.SizeAll,
                SnapsToDevicePixels = true
            };
            Canvas.SetLeft(energyContainer, underLeft);
            Canvas.SetTop(energyContainer, underTop);

            var energyBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 215, 64)), // Gold / Amber Accent
                BorderThickness = new Thickness(1.5),
                Background = new SolidColorBrush(Color.FromArgb(50, 255, 215, 64)),
                CornerRadius = new CornerRadius(4),
                IsHitTestVisible = true
            };
            energyContainer.Children.Add(energyBorder);

            // Badge for Energy box
            var energyBadge = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromArgb(220, 8, 14, 26)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 215, 64)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, -16, 0, 0),
                Padding = new Thickness(4, 1, 4, 1),
                IsHitTestVisible = false
            };
            energyBadge.Child = new TextBlock
            {
                Text = "⚡ Energy (Text OCR)",
                FontSize = 8.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 64)),
                VerticalAlignment = VerticalAlignment.Center
            };
            energyContainer.Children.Add(energyBadge);

            // Connector line
            var connectorLine = new Line
            {
                X1 = region.X + region.UnderOffsetX + 12,
                Y1 = region.Y + region.Height,
                X2 = region.X + region.UnderOffsetX + 12,
                Y2 = underTop,
                Stroke = new SolidColorBrush(Color.FromArgb(180, 255, 23, 68)),
                StrokeThickness = 1.2,
                StrokeDashArray = new DoubleCollection { 2, 2 },
                IsHitTestVisible = false
            };

            void UpdatePositions(double newEnemyX, double newEnemyY, double newEnemyH)
            {
                Canvas.SetLeft(enemyContainer, newEnemyX);
                Canvas.SetTop(enemyContainer, newEnemyY);

                double uL = newEnemyX + region.UnderOffsetX;
                double uT = newEnemyY + newEnemyH + region.UnderOffsetY;
                Canvas.SetLeft(energyContainer, uL);
                Canvas.SetTop(energyContainer, uT);

                connectorLine.X1 = uL + 12;
                connectorLine.Y1 = newEnemyY + newEnemyH;
                connectorLine.X2 = uL + 12;
                connectorLine.Y2 = uT;
            }

            if (!region.IsLocked)
            {
                // Resize grip for Enemy Bar
                var enemyResizeGrip = new Border
                {
                    Width = 14,
                    Height = 14,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = region.BorderBrush,
                    Opacity = 0.9,
                    Cursor = Cursors.SizeNWSE,
                    CornerRadius = new CornerRadius(3, 0, 3, 0),
                    ToolTip = "Drag to resize Enemy Bar"
                };
                enemyContainer.Children.Add(enemyResizeGrip);

                bool isEnemyResizing = false;
                Point startEnemyResizeMouse = default;
                double startEnemyW = 0, startEnemyH = 0;

                enemyResizeGrip.MouseLeftButtonDown += (s, e) =>
                {
                    isEnemyResizing = true;
                    startEnemyResizeMouse = e.GetPosition(this);
                    startEnemyW = region.Width;
                    startEnemyH = region.Height;
                    enemyResizeGrip.CaptureMouse();
                    e.Handled = true;
                };
                enemyResizeGrip.MouseMove += (s, e) =>
                {
                    if (!isEnemyResizing) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startEnemyResizeMouse.X;
                    double dY = cur.Y - startEnemyResizeMouse.Y;

                    double maxW = Math.Max(35, ActualWidth - region.X);
                    double maxH = Math.Max(8, ActualHeight - region.Y);

                    double newW = Math.Clamp(startEnemyW + dX, 35, maxW);
                    double newH = Math.Clamp(startEnemyH + dY, 8, maxH);

                    enemyContainer.Width = newW;
                    enemyContainer.Height = newH;
                    region.SetSizeSilently(newW, newH);
                    UpdatePositions(region.X, region.Y, newH);
                    e.Handled = true;
                };
                enemyResizeGrip.MouseLeftButtonUp += (s, e) =>
                {
                    if (isEnemyResizing)
                    {
                        isEnemyResizing = false;
                        enemyResizeGrip.ReleaseMouseCapture();
                        region.UpdateRelativePosition(ActualWidth, ActualHeight);
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };

                // Drag move for Enemy Bar
                bool isEnemyDragging = false;
                Point startEnemyDragMouse = default;
                double startEnemyX = 0, startEnemyY = 0;

                enemyContainer.MouseLeftButtonDown += (s, e) =>
                {
                    isEnemyDragging = true;
                    startEnemyDragMouse = e.GetPosition(this);
                    startEnemyX = region.X;
                    startEnemyY = region.Y;
                    enemyContainer.CaptureMouse();
                    e.Handled = true;
                };
                enemyContainer.MouseMove += (s, e) =>
                {
                    if (!isEnemyDragging) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startEnemyDragMouse.X;
                    double dY = cur.Y - startEnemyDragMouse.Y;

                    double maxW = Math.Max(0, ActualWidth - region.Width);
                    double maxH = Math.Max(0, ActualHeight - region.Height);

                    double newX = Math.Clamp(startEnemyX + dX, 0, maxW);
                    double newY = Math.Clamp(startEnemyY + dY, 0, maxH);

                    region.SetPositionSilently(newX, newY);
                    UpdatePositions(newX, newY, region.Height);
                    e.Handled = true;
                };
                enemyContainer.MouseLeftButtonUp += (s, e) =>
                {
                    if (isEnemyDragging)
                    {
                        isEnemyDragging = false;
                        enemyContainer.ReleaseMouseCapture();
                        region.UpdateRelativePosition(ActualWidth, ActualHeight);
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };

                // Resize grip for Energy Box
                var energyResizeGrip = new Border
                {
                    Width = 14,
                    Height = 14,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = new SolidColorBrush(Color.FromRgb(255, 215, 64)),
                    Opacity = 0.9,
                    Cursor = Cursors.SizeNWSE,
                    CornerRadius = new CornerRadius(3, 0, 3, 0),
                    ToolTip = "Drag to resize Energy Box"
                };
                energyContainer.Children.Add(energyResizeGrip);

                bool isEnergyResizing = false;
                Point startEnergyResizeMouse = default;
                double startEnergyW = 0, startEnergyH = 0;

                energyResizeGrip.MouseLeftButtonDown += (s, e) =>
                {
                    isEnergyResizing = true;
                    startEnergyResizeMouse = e.GetPosition(this);
                    startEnergyW = region.UnderWidth;
                    startEnergyH = region.UnderHeight;
                    energyResizeGrip.CaptureMouse();
                    e.Handled = true;
                };
                energyResizeGrip.MouseMove += (s, e) =>
                {
                    if (!isEnergyResizing) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startEnergyResizeMouse.X;
                    double dY = cur.Y - startEnergyResizeMouse.Y;

                    double newW = Math.Clamp(startEnergyW + dX, 20, 400);
                    double newH = Math.Clamp(startEnergyH + dY, 10, 150);

                    energyContainer.Width = newW;
                    energyContainer.Height = newH;
                    region.SetUnderSizeSilently(newW, newH);
                    e.Handled = true;
                };
                energyResizeGrip.MouseLeftButtonUp += (s, e) =>
                {
                    if (isEnergyResizing)
                    {
                        isEnergyResizing = false;
                        energyResizeGrip.ReleaseMouseCapture();
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };

                // Drag move for Energy Box offset
                bool isEnergyDragging = false;
                Point startEnergyDragMouse = default;
                double startOffsetX = 0, startOffsetY = 0;

                energyContainer.MouseLeftButtonDown += (s, e) =>
                {
                    isEnergyDragging = true;
                    startEnergyDragMouse = e.GetPosition(this);
                    startOffsetX = region.UnderOffsetX;
                    startOffsetY = region.UnderOffsetY;
                    energyContainer.CaptureMouse();
                    e.Handled = true;
                };
                energyContainer.MouseMove += (s, e) =>
                {
                    if (!isEnergyDragging) return;
                    Point cur = e.GetPosition(this);
                    double dX = cur.X - startEnergyDragMouse.X;
                    double dY = cur.Y - startEnergyDragMouse.Y;

                    double newOffsetX = Math.Clamp(startOffsetX + dX, -300, 500);
                    double newOffsetY = Math.Clamp(startOffsetY + dY, -50, 300);

                    region.SetUnderOffsetSilently(newOffsetX, newOffsetY);

                    double uL = region.X + newOffsetX;
                    double uT = region.Y + region.Height + newOffsetY;
                    Canvas.SetLeft(energyContainer, uL);
                    Canvas.SetTop(energyContainer, uT);

                    connectorLine.X1 = uL + 12;
                    connectorLine.Y1 = region.Y + region.Height;
                    connectorLine.X2 = uL + 12;
                    connectorLine.Y2 = uT;

                    e.Handled = true;
                };
                energyContainer.MouseLeftButtonUp += (s, e) =>
                {
                    if (isEnergyDragging)
                    {
                        isEnergyDragging = false;
                        energyContainer.ReleaseMouseCapture();
                        region.NotifyPositionChanged();
                        e.Handled = true;
                    }
                };
            }

            canvas.Children.Add(connectorLine);
            canvas.Children.Add(enemyContainer);
            canvas.Children.Add(energyContainer);
        }

        private FrameworkElement CreateSubRegionVisual(SubRegion region)
        {
            // Position for pinned region
            if (region.IsPinnedTopCenter && ActualWidth > 0)
            {
                region.X = Math.Max(0, (ActualWidth - region.Width) / 2);
                region.Y = 8;
            }

            var container = new Grid
            {
                Width = region.Width,
                Height = region.Height,
                Cursor = (region.IsLocked || region.IsPinnedTopCenter) ? Cursors.Arrow : Cursors.SizeAll,
                SnapsToDevicePixels = true
            };

            Canvas.SetLeft(container, region.X);
            Canvas.SetTop(container, region.Y);

            // Outer border
            var border = new Border
            {
                BorderBrush = region.BorderBrush,
                BorderThickness = new Thickness(region.IsLocked ? 2.5 : 1.5),
                Background = region.FillBrush,
                CornerRadius = new CornerRadius(4),
                IsHitTestVisible = true
            };
            container.Children.Add(border);

            // Internal Grid (only if region.HasGrid is true)
            Canvas? innerGridCanvas = null;
            if (region.HasGrid)
            {
                innerGridCanvas = new Canvas { IsHitTestVisible = false };
                RenderInternalGrid(innerGridCanvas, region);
                container.Children.Add(innerGridCanvas);
            }

            // Header badge (Top-Left) — For R1 Healthbar, placed ABOVE the box so it never covers text
            var badgeBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromArgb(220, 8, 14, 26)),
                BorderBrush = region.BorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = region.Id == 1 ? new Thickness(0, -18, 0, 0) : new Thickness(3),
                Padding = new Thickness(5, 1, 5, 1),
                IsHitTestVisible = false
            };

            var badgeStack = new StackPanel { Orientation = Orientation.Horizontal };
            if (region.IsLocked)
            {
                badgeStack.Children.Add(new TextBlock
                {
                    Text = "🔒 ",
                    FontSize = 9.5,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            else if (region.IsPinnedTopCenter)
            {
                badgeStack.Children.Add(new TextBlock
                {
                    Text = "📌 ",
                    FontSize = 9.5,
                    Foreground = region.BorderBrush,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            else
            {
                badgeStack.Children.Add(new TextBlock
                {
                    Text = "✥ ",
                    FontSize = 9.5,
                    Foreground = region.BorderBrush,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            // Only region name, without NxM text!
            badgeStack.Children.Add(new TextBlock
            {
                Text = region.Name,
                FontSize = 9.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            });
            badgeBorder.Child = badgeStack;
            container.Children.Add(badgeBorder);

            // Resize handle (Bottom-Right corner) — works for all unlocked regions, including R5
            if (!region.IsLocked)
            {
                var resizeGrip = new Border
                {
                    Width = 16,
                    Height = 16,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = region.BorderBrush,
                    Opacity = 0.9,
                    Cursor = Cursors.SizeNWSE,
                    CornerRadius = new CornerRadius(4, 0, 4, 0),
                    ToolTip = "Drag to resize area"
                };

                SetupResizeEvents(resizeGrip, container, innerGridCanvas, region);
                container.Children.Add(resizeGrip);

                // Setup drag move (only for regions that are NOT pinned to top-center)
                if (!region.IsPinnedTopCenter)
                {
                    SetupDragEvents(container, region);
                }
            }

            return container;
        }

        private static void RenderInternalGrid(Canvas gridCanvas, SubRegion region)
        {
            gridCanvas.Children.Clear();
            if (!region.HasGrid || !region.IsGridVisible || region.Rows <= 0 || region.Columns <= 0) return;

            double cellW = region.Width / region.Columns;
            double cellH = region.Height / region.Rows;

            for (int r = 0; r < region.Rows; r++)
            {
                for (int c = 0; c < region.Columns; c++)
                {
                    var cellRect = new Rectangle
                    {
                        Width = cellW,
                        Height = cellH,
                        Fill = (r + c) % 2 == 0 ? region.CellBrushA : region.CellBrushB
                    };
                    Canvas.SetLeft(cellRect, c * cellW);
                    Canvas.SetTop(cellRect, r * cellH);
                    gridCanvas.Children.Add(cellRect);
                }
            }

            for (int c = 1; c < region.Columns; c++)
            {
                gridCanvas.Children.Add(new Line
                {
                    X1 = c * cellW, Y1 = 0,
                    X2 = c * cellW, Y2 = region.Height,
                    Stroke = region.GridLineBrush,
                    StrokeThickness = 0.8
                });
            }

            for (int r = 1; r < region.Rows; r++)
            {
                gridCanvas.Children.Add(new Line
                {
                    X1 = 0, Y1 = r * cellH,
                    X2 = region.Width, Y2 = r * cellH,
                    Stroke = region.GridLineBrush,
                    StrokeThickness = 0.8
                });
            }
        }

        private void SetupDragEvents(Grid container, SubRegion region)
        {
            bool isDragging = false;
            Point startMouse = default;
            double startX = 0, startY = 0;

            container.MouseLeftButtonDown += (s, e) =>
            {
                if (region.IsLocked || region.IsPinnedTopCenter) return;

                isDragging = true;
                startMouse = e.GetPosition(this);
                startX = region.X;
                startY = region.Y;
                container.CaptureMouse();
                e.Handled = true;
            };

            container.MouseMove += (s, e) =>
            {
                if (!isDragging) return;

                Point current = e.GetPosition(this);
                double deltaX = current.X - startMouse.X;
                double deltaY = current.Y - startMouse.Y;

                double maxW = Math.Max(0, ActualWidth - region.Width);
                double maxH = Math.Max(0, ActualHeight - region.Height);

                double newX = Math.Clamp(startX + deltaX, 0, maxW);
                double newY = Math.Clamp(startY + deltaY, 0, maxH);

                Canvas.SetLeft(container, newX);
                Canvas.SetTop(container, newY);

                region.SetPositionSilently(newX, newY);
                e.Handled = true;
            };

            container.MouseLeftButtonUp += (s, e) =>
            {
                if (isDragging)
                {
                    isDragging = false;
                    container.ReleaseMouseCapture();
                    region.UpdateRelativePosition(ActualWidth, ActualHeight);
                    region.NotifyPositionChanged();
                    e.Handled = true;
                }
            };
        }

        private void SetupResizeEvents(Border resizeGrip, Grid container, Canvas? innerGridCanvas, SubRegion region)
        {
            bool isResizing = false;
            Point startMouse = default;
            double startW = 0, startH = 0;

            resizeGrip.MouseLeftButtonDown += (s, e) =>
            {
                if (region.IsLocked) return;

                isResizing = true;
                startMouse = e.GetPosition(this);
                startW = region.Width;
                startH = region.Height;
                resizeGrip.CaptureMouse();
                e.Handled = true;
            };

            resizeGrip.MouseMove += (s, e) =>
            {
                if (!isResizing) return;
                Point current = e.GetPosition(this);
                double deltaX = current.X - startMouse.X;
                double deltaY = current.Y - startMouse.Y;

                if (region.IsPinnedTopCenter)
                {
                    // R5 is pinned to top-center: resize width symmetrically while keeping centered
                    double newW = Math.Clamp(startW + deltaX * 2, 60, ActualWidth - 20);
                    double newH = Math.Clamp(startH + deltaY, 20, 150);

                    container.Width = newW;
                    container.Height = newH;
                    region.SetSizeSilently(newW, newH);

                    double newX = Math.Max(0, (ActualWidth - newW) / 2);
                    Canvas.SetLeft(container, newX);
                    region.SetPositionSilently(newX, 8);
                }
                else
                {
                    double maxW = ActualWidth - region.X;
                    double maxH = ActualHeight - region.Y;

                    double newW = Math.Clamp(startW + deltaX, 35, maxW);
                    double newH = Math.Clamp(startH + deltaY, 25, maxH);

                    container.Width = newW;
                    container.Height = newH;
                    region.SetSizeSilently(newW, newH);
                }

                if (innerGridCanvas != null && region.HasGrid)
                {
                    RenderInternalGrid(innerGridCanvas, region);
                }
                e.Handled = true;
            };

            resizeGrip.MouseLeftButtonUp += (s, e) =>
            {
                if (isResizing)
                {
                    isResizing = false;
                    resizeGrip.ReleaseMouseCapture();
                    region.UpdateRelativePosition(ActualWidth, ActualHeight);
                    region.NotifyPositionChanged();
                    e.Handled = true;
                }
            };
        }

        // Redraw grid & scale locked sub-regions when MR is resized
        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);

            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            foreach (var region in _subRegions)
            {
                if (region.IsPinnedTopCenter)
                {
                    region.X = Math.Max(0, (w - region.Width) / 2);
                    region.Y = 8;
                    region.UpdateRelativePosition(w, h);
                }
                else if (region.IsLocked)
                {
                    region.ApplyRelativePosition(w, h);
                }
                else
                {
                    if (region.X + region.Width > w)
                    {
                        region.X = Math.Max(0, w - region.Width);
                    }
                    if (region.Y + region.Height > h)
                    {
                        region.Y = Math.Max(0, h - region.Height);
                    }
                    region.UpdateRelativePosition(w, h);
                }
            }

            if (_gridVisible) DrawGrid();
            RedrawSubRegions();
        }
    }
}
