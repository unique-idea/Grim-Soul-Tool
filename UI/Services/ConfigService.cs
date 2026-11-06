using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using UI.Models;
using UI.Windows;

namespace UI.Services
{
    /// <summary>
    /// Service to load and save window coordinates, dimensions, and sub-region settings to config.json.
    /// </summary>
    public static class ConfigService
    {
        private const string ConfigFileName = "config.json";

        public static string GetConfigFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        }

        /// <summary>
        /// Attempts to load configuration from candidate directories.
        /// </summary>
        public static AppConfiguration? Load()
        {
            try
            {
                foreach (var path in GetCandidatePaths())
                {
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        var config = JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true
                        });

                        if (config != null)
                        {
                            Debug.WriteLine($"[ConfigService] Configuration loaded from: {path}");
                            return config;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigService] Load error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Saves the current MR position/size and all SubRegion properties to config.json.
        /// </summary>
        public static bool Save(OverlayWindow overlay, ItemListWindow? itemList = null)
        {
            try
            {
                var existingConfig = Load();
                var config = new AppConfiguration
                {
                    MainRegion = new MainRegionConfig
                    {
                        Left = overlay.Left,
                        Top = overlay.Top,
                        Width = overlay.ActualWidth > 0 ? overlay.ActualWidth : overlay.Width,
                        Height = overlay.ActualHeight > 0 ? overlay.ActualHeight : overlay.Height
                    },
                    ItemSettings = itemList != null ? itemList.GetItemSettings() : (existingConfig?.ItemSettings ?? new Dictionary<string, bool>())
                };

                foreach (var r in overlay.SubRegions)
                {
                    config.SubRegions.Add(new SubRegionConfig
                    {
                        Id = r.Id,
                        Name = r.Name,
                        IsVisible = r.IsVisible,
                        IsGridVisible = r.IsGridVisible,
                        IsLocked = r.IsLocked,
                        X = r.X,
                        Y = r.Y,
                        Width = r.Width,
                        Height = r.Height,
                        Rows = r.Rows,
                        Columns = r.Columns,
                        ScanIntervalMs = r.ScanIntervalMs,
                        MaxHealthValue = r.MaxHealthValue,
                        MaxEnergyValue = r.MaxEnergyValue,
                        EnemyThresholdPercent = r.EnemyThresholdPercent,
                        HealCooldownMs = r.HealCooldownMs,
                        HealThresholdPercent = r.HealThresholdPercent,
                        HealthBarSplitPercent = r.HealthBarSplitPercent,
                        UnderWidth = r.UnderWidth,
                        UnderHeight = r.UnderHeight,
                        UnderOffsetX = r.UnderOffsetX,
                        UnderOffsetY = r.UnderOffsetY
                    });
                }

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Save to application directory
                string primaryPath = GetConfigFilePath();
                File.WriteAllText(primaryPath, json);

                // Also save to project directory if in dev mode so bin clean doesn't wipe it
                foreach (var projPath in GetProjectCandidatePaths())
                {
                    try
                    {
                        File.WriteAllText(projPath, json);
                    }
                    catch { }
                }

                Debug.WriteLine($"[ConfigService] Configuration successfully saved to: {primaryPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigService] Save error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Applies loaded configuration to the OverlayWindow and its SubRegions.
        /// </summary>
        
        /// <summary>
        /// Saves updated Item whitelist settings (checked/unchecked) directly to config.json.
        /// </summary>
        public static bool SaveItemSettings(Dictionary<string, bool> itemSettings)
        {
            try
            {
                var config = Load() ?? new AppConfiguration();
                config.ItemSettings = itemSettings;

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                string primaryPath = GetConfigFilePath();
                File.WriteAllText(primaryPath, json);

                foreach (var projPath in GetProjectCandidatePaths())
                {
                    try
                    {
                        File.WriteAllText(projPath, json);
                    }
                    catch { }
                }

                Debug.WriteLine($"[ConfigService] Item settings saved to config.json ({itemSettings.Count} items).");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigService] SaveItemSettings error: {ex.Message}");
                return false;
            }
        }

        public static void ApplyToOverlay(AppConfiguration config, OverlayWindow overlay)
        {
            try
            {
                if (config.MainRegion != null && config.MainRegion.Width >= 160 && config.MainRegion.Height >= 120)
                {
                    double screenW = SystemParameters.VirtualScreenWidth;
                    double screenH = SystemParameters.VirtualScreenHeight;
                    double screenLeft = SystemParameters.VirtualScreenLeft;
                    double screenTop = SystemParameters.VirtualScreenTop;

                    if (config.MainRegion.Left >= screenLeft - 50 && config.MainRegion.Left < screenLeft + screenW &&
                        config.MainRegion.Top >= screenTop - 50 && config.MainRegion.Top < screenTop + screenH)
                    {
                        overlay.WindowStartupLocation = WindowStartupLocation.Manual;
                        overlay.Left = config.MainRegion.Left;
                        overlay.Top = config.MainRegion.Top;
                    }

                    overlay.Width = Math.Max(160, config.MainRegion.Width);
                    overlay.Height = Math.Max(120, config.MainRegion.Height);
                }

                if (config.SubRegions != null)
                {
                    foreach (var subConfig in config.SubRegions)
                    {
                        var r = overlay.SubRegions.FirstOrDefault(x => x.Id == subConfig.Id);
                        if (r != null)
                        {
                            r.X = subConfig.X;
                            r.Y = subConfig.Y;
                            r.Width = subConfig.Width;
                            r.Height = subConfig.Height;
                            r.IsVisible = subConfig.IsVisible;
                            r.IsGridVisible = subConfig.IsGridVisible;
                            r.IsLocked = subConfig.IsLocked;
                            if (r.Id == 3 || r.Id == 6)
                            {
                                r.Rows = 5;
                                r.Columns = 5;
                                r.HasGridSettings = false;
                                r.HasInterval = false;
                            }
                            else
                            {
                                r.Rows = subConfig.Rows;
                                r.Columns = subConfig.Columns;
                            }
                            r.ScanIntervalMs = subConfig.ScanIntervalMs;
                            r.MaxHealthValue = subConfig.MaxHealthValue;
                            r.MaxEnergyValue = subConfig.MaxEnergyValue;
                            r.EnemyThresholdPercent = subConfig.EnemyThresholdPercent > 0 ? subConfig.EnemyThresholdPercent : 50;
                            r.HealCooldownMs = subConfig.HealCooldownMs;
                            r.HealThresholdPercent = subConfig.HealThresholdPercent;
                            r.HealthBarSplitPercent = subConfig.HealthBarSplitPercent;
                            r.UnderWidth = subConfig.UnderWidth;
                            r.UnderHeight = subConfig.UnderHeight;
                            r.UnderOffsetX = subConfig.UnderOffsetX;
                            r.UnderOffsetY = subConfig.UnderOffsetY;
                        }
                    }
                }

                overlay.HasLoadedConfig = true;

                if (overlay.IsLoaded)
                {
                    overlay.RedrawSubRegions();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigService] Apply error: {ex.Message}");
            }
        }

        private static IEnumerable<string> GetCandidatePaths()
        {
            yield return GetConfigFilePath();
            yield return Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);

            foreach (var projPath in GetProjectCandidatePaths())
            {
                yield return projPath;
            }
        }

        private static IEnumerable<string> GetProjectCandidatePaths()
        {
            string? dir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 5 && dir != null; i++)
            {
                dir = Directory.GetParent(dir)?.FullName;
                if (dir != null)
                {
                    if (File.Exists(Path.Combine(dir, "UI.csproj")) || Directory.Exists(Path.Combine(dir, "UI")))
                    {
                        yield return Path.Combine(dir, ConfigFileName);
                    }
                }
            }
        }
    }
}
