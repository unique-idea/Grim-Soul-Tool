using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace UI.Models
{
    /// <summary>
    /// Represents a sub-region rectangle inside the Main Rectangle (MR).
    /// Used to designate specific scan areas (e.g. Health bar, Mini-map, Inventory, Equipment, Enemy bar).
    /// </summary>
    public class SubRegion : INotifyPropertyChanged
    {
        public int Id { get; init; }
        public string Name { get; set; } = string.Empty;

        // If false, no internal grid is drawn (e.g. Healthbar, Enemy Bar)
        public bool HasGrid { get; set; } = true;

        // If false, row/column configuration controls are not needed (e.g. Healthbar, Enemy Bar, fixed Equipment 5x1, fixed Inventory 5x5)
        public bool HasGridSettings { get; set; } = true;

        // If false, this region does not run on a periodic timer interval (e.g. Inventory & Chest scanned on-demand/event)
        public bool HasInterval { get; set; } = true;

        /// <summary>
        /// Background colors of empty / occupied chest and loot slots in Region 6 (R6).
        /// </summary>
        public static readonly string[] R6SlotBackgroundHex = new[]
        {
            "#2B2B2B", "#434443", "#383837", "#2B2C2B", "#2A2928"
        };

        // If true, locked in middle of top MR (cannot be moved freely around MR, only resized in W & H)
        public bool IsPinnedTopCenter { get; set; } = false;

        private Color _themeColor;
        public Color ThemeColor
        {
            get => _themeColor;
            set
            {
                if (_themeColor != value)
                {
                    _themeColor = value;
                    OnPropertyChanged();
                    UpdateBrushes();
                    OnChanged();
                }
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged(); OnChanged(); } }
        }

        private bool _isGridVisible = true;
        public bool IsGridVisible
        {
            get => _isGridVisible;
            set { if (_isGridVisible != value) { _isGridVisible = value; OnPropertyChanged(); OnChanged(); } }
        }

        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set { if (_isLocked != value) { _isLocked = value; OnPropertyChanged(); OnChanged(); } }
        }

        private int _scanIntervalMs = 1000;
        public int ScanIntervalMs
        {
            get => _scanIntervalMs;
            set
            {
                int val = Math.Clamp(value, 50, 60000);
                if (_scanIntervalMs != val)
                {
                    _scanIntervalMs = val;
                    OnPropertyChanged();
                    OnChanged();
                }
            }
        }

        private int _maxHealthValue = 120;
        public int MaxHealthValue
        {
            get => _maxHealthValue;
            set
            {
                int val = Math.Clamp(value, 1, 9999);
                if (_maxHealthValue != val)
                {
                    _maxHealthValue = val;
                    OnPropertyChanged();
                    OnChanged();
                }
            }
        }

        private int _healCooldownMs = 1000;
        public int HealCooldownMs
        {
            get => _healCooldownMs;
            set
            {
                int val = Math.Clamp(value, 200, 30000);
                if (_healCooldownMs != val)
                {
                    _healCooldownMs = val;
                    Engine.Services.Implements.Action.HealCooldownMs = val;
                    OnPropertyChanged();
                    OnChanged();
                }
            }
        }

        private int _healThresholdPercent = 40;
        public int HealThresholdPercent
        {
            get => _healThresholdPercent;
            set
            {
                int val = Math.Clamp(value, 5, 95);
                if (_healThresholdPercent != val)
                {
                    _healThresholdPercent = val;
                    OnPropertyChanged();
                    OnChanged();
                }
            }
        }

        private int _healthBarSplitPercent = 25; // % height for Health Bar (top), remaining (100 - val)% for Text (bottom)
        public int HealthBarSplitPercent
        {
            get => _healthBarSplitPercent;
            set
            {
                int val = Math.Clamp(value, 5, 95);
                if (_healthBarSplitPercent != val)
                {
                    _healthBarSplitPercent = val;
                    OnPropertyChanged();
                    OnChanged();
                }
            }
        }

        private int _maxEnergyValue = 100;
        public int MaxEnergyValue
        {
            get => _maxEnergyValue;
            set
            {
                int val = Math.Clamp(value, 1, 9999);
                if (_maxEnergyValue != val)
                {
                    _maxEnergyValue = val;
                    OnPropertyChanged();
                    OnChanged();
                }
            }
        }

        private int _enemyThresholdPercent = 50;
        public int EnemyThresholdPercent
        {
            get => _enemyThresholdPercent;
            set
            {
                int val = Math.Clamp(value, 10, 95);
                if (_enemyThresholdPercent != val)
                {
                    _enemyThresholdPercent = val;
                    OnPropertyChanged();
                    OnChanged();
                }
            }
        }

        // ─── Under Region for Healthbar (Exact Current HP Text Box) ───────
        private double _underWidth = 65;
        public double UnderWidth
        {
            get => _underWidth;
            set { if (Math.Abs(_underWidth - value) > 0.001) { _underWidth = Math.Max(20, value); OnPropertyChanged(); OnChanged(); } }
        }

        private double _underHeight = 22;
        public double UnderHeight
        {
            get => _underHeight;
            set { if (Math.Abs(_underHeight - value) > 0.001) { _underHeight = Math.Max(10, value); OnPropertyChanged(); OnChanged(); } }
        }

        private double _underOffsetX = 0;
        public double UnderOffsetX
        {
            get => _underOffsetX;
            set { if (Math.Abs(_underOffsetX - value) > 0.001) { _underOffsetX = value; OnPropertyChanged(); OnChanged(); } }
        }

        private double _underOffsetY = 3;
        public double UnderOffsetY
        {
            get => _underOffsetY;
            set { if (Math.Abs(_underOffsetY - value) > 0.001) { _underOffsetY = value; OnPropertyChanged(); OnChanged(); } }
        }

        private int _rows = 3;
        public int Rows
        {
            get => _rows;
            set { int val = Math.Clamp(value, 1, 50); if (_rows != val) { _rows = val; OnPropertyChanged(); OnChanged(); } }
        }

        private int _columns = 3;
        public int Columns
        {
            get => _columns;
            set { int val = Math.Clamp(value, 1, 50); if (_columns != val) { _columns = val; OnPropertyChanged(); OnChanged(); } }
        }

        private double _x;
        public double X
        {
            get => _x;
            set { if (Math.Abs(_x - value) > 0.001) { _x = value; OnPropertyChanged(); OnChanged(); } }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set { if (Math.Abs(_y - value) > 0.001) { _y = value; OnPropertyChanged(); OnChanged(); } }
        }

        private double _width = 120;
        public double Width
        {
            get => _width;
            set { if (Math.Abs(_width - value) > 0.001) { _width = Math.Max(30, value); OnPropertyChanged(); OnChanged(); } }
        }

        private double _height = 80;
        public double Height
        {
            get => _height;
            set { if (Math.Abs(_height - value) > 0.001) { _height = Math.Max(20, value); OnPropertyChanged(); OnChanged(); } }
        }

        // Relative coordinates (0.0 - 1.0) inside MR for preserving position on MR resize when locked
        public double RelX { get; set; }
        public double RelY { get; set; }
        public double RelW { get; set; }
        public double RelH { get; set; }

        // Cached brushes for UI performance
        public SolidColorBrush BorderBrush { get; private set; } = Brushes.Transparent;
        public SolidColorBrush FillBrush { get; private set; } = Brushes.Transparent;
        public SolidColorBrush GridLineBrush { get; private set; } = Brushes.Transparent;
        public SolidColorBrush CellBrushA { get; private set; } = Brushes.Transparent;
        public SolidColorBrush CellBrushB { get; private set; } = Brushes.Transparent;

        public event EventHandler? Changed;
        public event EventHandler? PositionChanged;

        public SubRegion(int id, string name, Color themeColor, double x, double y, double width, double height, int rows, int columns)
        {
            Id = id;
            Name = name;
            _themeColor = themeColor;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _rows = rows;
            _columns = columns;

            UpdateBrushes();
        }

        private void UpdateBrushes()
        {
            BorderBrush = new SolidColorBrush(_themeColor);
            BorderBrush.Freeze();

            FillBrush = new SolidColorBrush(Color.FromArgb(14, _themeColor.R, _themeColor.G, _themeColor.B));
            FillBrush.Freeze();

            GridLineBrush = new SolidColorBrush(Color.FromArgb(55, _themeColor.R, _themeColor.G, _themeColor.B));
            GridLineBrush.Freeze();

            CellBrushA = new SolidColorBrush(Color.FromArgb(10, _themeColor.R, _themeColor.G, _themeColor.B));
            CellBrushA.Freeze();

            byte rAlt = (byte)Math.Min(255, _themeColor.R + 25);
            byte gAlt = (byte)Math.Min(255, _themeColor.G + 25);
            byte bAlt = (byte)Math.Min(255, _themeColor.B + 25);
            CellBrushB = new SolidColorBrush(Color.FromArgb(18, rAlt, gAlt, bAlt));
            CellBrushB.Freeze();
        }

        public void SetPositionSilently(double x, double y)
        {
            _x = x;
            _y = y;
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        public void SetSizeSilently(double w, double h)
        {
            _width = Math.Max(30, w);
            _height = Math.Max(8, h);
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
        }

        public void SetUnderSizeSilently(double w, double h)
        {
            _underWidth = Math.Max(20, w);
            _underHeight = Math.Max(10, h);
            OnPropertyChanged(nameof(UnderWidth));
            OnPropertyChanged(nameof(UnderHeight));
        }

        public void SetUnderOffsetSilently(double ox, double oy)
        {
            _underOffsetX = ox;
            _underOffsetY = oy;
            OnPropertyChanged(nameof(UnderOffsetX));
            OnPropertyChanged(nameof(UnderOffsetY));
        }

        public void NotifyPositionChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            PositionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateRelativePosition(double mrWidth, double mrHeight)
        {
            if (mrWidth > 0 && mrHeight > 0)
            {
                RelX = X / mrWidth;
                RelY = Y / mrHeight;
                RelW = Width / mrWidth;
                RelH = Height / mrHeight;
            }
        }

        public void ApplyRelativePosition(double mrWidth, double mrHeight)
        {
            if (mrWidth > 0 && mrHeight > 0 && RelW > 0 && RelH > 0)
            {
                if (IsPinnedTopCenter)
                {
                    _width = Math.Clamp(RelW * mrWidth, 40, mrWidth);
                    _height = Math.Clamp(RelH * mrHeight, 20, mrHeight);
                    _x = Math.Max(0, (mrWidth - _width) / 2);
                    _y = 8;
                }
                else
                {
                    _width = Math.Clamp(RelW * mrWidth, 30, mrWidth);
                    _height = Math.Clamp(RelH * mrHeight, 20, mrHeight);
                    _x = Math.Clamp(RelX * mrWidth, 0, mrWidth - _width);
                    _y = Math.Clamp(RelY * mrHeight, 0, mrHeight - _height);
                }

                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(Y));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
                NotifyPositionChanged();
            }
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
