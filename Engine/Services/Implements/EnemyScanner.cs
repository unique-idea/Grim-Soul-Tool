using Engine.Models;
using Engine.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Engine.Services.Implements
{
    public class EnemyScanner : IEnemyScanner
    {
        private readonly TesseractOcrDetector _tesseractDetector;

        public EnemyScanner(TesseractOcrDetector? tesseractDetector = null)
        {
            _tesseractDetector = tesseractDetector ?? new TesseractOcrDetector();
        }

        public async Task<EnemyEnergyInfo> ScanEnemyAndEnergyFromScreenAsync(
            int enemyX, int enemyY, int enemyW, int enemyH,
            int energyX, int energyY, int energyW, int energyH,
            int maxEnergy = 100)
        {
            if (maxEnergy <= 0) maxEnergy = 100;
            var info = new EnemyEnergyInfo { MaxEnergy = maxEnergy };

            return await Task.Run(() =>
            {
                // 1. Scan Enemy Bar for red line
                if (enemyW > 4 && enemyH > 4)
                {
                    try
                    {
                        using var enemyBmp = new Bitmap(enemyW, enemyH, PixelFormat.Format32bppArgb);
                        using (var g = Graphics.FromImage(enemyBmp))
                        {
                            g.CopyFromScreen(enemyX, enemyY, 0, 0, new Size(enemyW, enemyH), CopyPixelOperation.SourceCopy);
                        }

                        double redPercent = HealthBarScanner.MeasureRedBarPercentageHSV(enemyBmp);
                        info.EnemyHpPercentage = Math.Round(redPercent, 1);
                        info.HasEnemy = redPercent >= 3.0;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[EnemyScanner Error]: {ex.Message}");
                    }
                }

                // 2. Scan Energy Text Region via OCR
                if (energyW > 4 && energyH > 4)
                {
                    try
                    {
                        using var energyBmp = new Bitmap(energyW, energyH, PixelFormat.Format32bppArgb);
                        using (var g = Graphics.FromImage(energyBmp))
                        {
                            g.CopyFromScreen(energyX, energyY, 0, 0, new Size(energyW, energyH), CopyPixelOperation.SourceCopy);
                        }

                        if (_tesseractDetector != null && _tesseractDetector.IsAvailable)
                        {
                            var (val, rawText) = _tesseractDetector.RecognizeCurrentHpOnly(energyBmp, maxEnergy);
                            if (val.HasValue && val.Value >= 0)
                            {
                                info.CurrentEnergy = Math.Clamp(val.Value, 0, maxEnergy);
                                info.EnergyPercentage = Math.Round((info.CurrentEnergy / (double)maxEnergy) * 100.0, 1);
                            }
                            info.RawEnergyText = rawText;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[EnergyScanner OCR Error]: {ex.Message}");
                    }
                }

                return info;
            });
        }

        public bool CheckEnemyRedBarFromScreen(int enemyX, int enemyY, int enemyW, int enemyH)
        {
            if (enemyW <= 4 || enemyH <= 4) return false;

            try
            {
                using var bmp = new Bitmap(enemyW, enemyH, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(enemyX, enemyY, 0, 0, new Size(enemyW, enemyH), CopyPixelOperation.SourceCopy);
                }

                double redPercent = HealthBarScanner.MeasureRedBarPercentageHSV(bmp);
                return redPercent >= 3.0;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _tesseractDetector?.Dispose();
        }
    }
}
