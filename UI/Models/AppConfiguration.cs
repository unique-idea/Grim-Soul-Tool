using System.Collections.Generic;

namespace UI.Models
{
    /// <summary>
    /// Configuration for Main Region (MR / OverlayWindow).
    /// </summary>
    public class MainRegionConfig
    {
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public double Width { get; set; } = 960;
        public double Height { get; set; } = 540;
    }

    /// <summary>
    /// Configuration for each individual SubRegion (R1..R5).
    /// </summary>
    public class SubRegionConfig
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public bool IsGridVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        // Position & Size relative to MR
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        // Grid rows and columns
        public int Rows { get; set; } = 1;
        public int Columns { get; set; } = 1;

        // Pipeline Scan interval in ms
        public int ScanIntervalMs { get; set; } = 1000;

        // Health bar specific settings (R1)
        public int MaxHealthValue { get; set; } = 120;
        public int MaxEnergyValue { get; set; } = 100;
        public int EnemyThresholdPercent { get; set; } = 50;
        public int HealCooldownMs { get; set; } = 1000;
        public int HealThresholdPercent { get; set; } = 40;
        public int HealthBarSplitPercent { get; set; } = 25;
        public double UnderWidth { get; set; } = 65;
        public double UnderHeight { get; set; } = 22;
        public double UnderOffsetX { get; set; } = 0;
        public double UnderOffsetY { get; set; } = 4;
    }

    /// <summary>
    /// Root application configuration persisted in config.json.
    /// </summary>
    public class AppConfiguration
    {
        public MainRegionConfig MainRegion { get; set; } = new();
        public List<SubRegionConfig> SubRegions { get; set; } = new();
        public Dictionary<string, bool> ItemSettings { get; set; } = new();
    }
}
