using Engine.Enums;
using Engine.Models;
using Engine.References.Models;
using Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Services.Implements
{
    public class ChestScanner : IChestScanner
    {
        private readonly IDetector _detector;
        private readonly TesseractOcrDetector? _tesseractDetector;

        public static readonly IReadOnlyList<Color> ChestSlotBackgroundColors = new List<Color>
        {
            ColorTranslator.FromHtml("#2B2B2B"),
            ColorTranslator.FromHtml("#2C2C2B"),
            ColorTranslator.FromHtml("#2B2C2C"),
            ColorTranslator.FromHtml("#2A2B2B"),
            ColorTranslator.FromHtml("#2B2B2A"),
            ColorTranslator.FromHtml("#2C2C2C"),
            ColorTranslator.FromHtml("#2C2B2B"),
            ColorTranslator.FromHtml("#2A2B2C"),

            ColorTranslator.FromHtml("#373939"),
            ColorTranslator.FromHtml("#36393A"),
            ColorTranslator.FromHtml("#373B3B"),
            ColorTranslator.FromHtml("#373C3D"),
            ColorTranslator.FromHtml("#353A3B"),
            ColorTranslator.FromHtml("#363B3C"),
            ColorTranslator.FromHtml("#383B3B")
        };

        public static readonly IReadOnlyList<Color> ChestGridMatrixColors = new List<Color>
        {
            ColorTranslator.FromHtml("#4D443C"),
            ColorTranslator.FromHtml("#433C35"),
            ColorTranslator.FromHtml("#3A342E"),
            ColorTranslator.FromHtml("#544B42")
        };

        public ChestScanner(IDetector? detector = null, TesseractOcrDetector? tesseractDetector = null)
        {
            _detector = detector ?? new Detector();
            _tesseractDetector = tesseractDetector;
        }

        public bool IsChestOpen(int chestX, int chestY, int chestW, int chestH)
        {
            if (chestW <= 20 || chestH <= 20) return false;

            try
            {
                using var bmp = new Bitmap(chestW, chestH, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(chestX, chestY, 0, 0, new Size(chestW, chestH), CopyPixelOperation.SourceCopy);
                }
                return CheckIsChestOpenFromBitmap(bmp);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChestScanner IsChestOpen Error]: {ex.Message}");
                return false;
            }
        }

        public bool CheckIsChestOpenFromBitmap(Bitmap bmp)
        {
            if (bmp == null || bmp.Width <= 20 || bmp.Height <= 20) return false;

            int totalSamples = 0;
            int validChestBgMatches = 0;
            int nightTintMatches = 0;
            int matrixGridMatches = 0;

            int stepX = Math.Max(2, bmp.Width / 35);
            int stepY = Math.Max(2, bmp.Height / 35);

            for (int y = stepY; y < bmp.Height - stepY; y += stepY)
            {
                for (int x = stepX; x < bmp.Width - stepX; x += stepX)
                {
                    Color px = bmp.GetPixel(x, y);
                    totalSamples++;

                    if (px.B > px.R + 8 && px.B > px.G + 6)
                    {
                        nightTintMatches++;
                    }

                    if (IsColorMatch(px, ChestSlotBackgroundColors, 14.0))
                    {
                        int maxDiff = Math.Max(Math.Abs(px.R - px.G), Math.Max(Math.Abs(px.G - px.B), Math.Abs(px.R - px.B)));
                        if (maxDiff <= 6)
                        {
                            validChestBgMatches++;
                        }
                    }

                    if (IsColorMatch(px, ChestGridMatrixColors, 12.0))
                    {
                        matrixGridMatches++;
                    }
                }
            }

            if (totalSamples == 0) return false;

            double nightRatio = (double)nightTintMatches / totalSamples;
            double chestBgRatio = (double)validChestBgMatches / totalSamples;
            double matrixRatio = (double)matrixGridMatches / totalSamples;

            if (nightRatio >= 0.12)
            {
                Debug.WriteLine($"[Chest Check] Rejected: Night Blue Tint Detected ({nightRatio:P1})");
                return false;
            }

            bool isOpen = (chestBgRatio >= 0.20) && (matrixRatio >= 0.015 || chestBgRatio >= 0.40);
            return isOpen;
        }

        private static bool IsColorMatch(Color pixel, IEnumerable<Color> targetColors, double tolerance)
        {
            foreach (var target in targetColors)
            {
                if (CalculateColorDistance(pixel, target) <= tolerance)
                    return true;
            }
            return false;
        }

        public List<ChestSlotResult> ScanChestSlots(int chestX, int chestY, int chestW, int chestH, IEnumerable<Item>? candidateItems = null)
        {
            var results = new List<ChestSlotResult>();
            if (chestW <= 20 || chestH <= 20) return results;

            try
            {
                using var bmp = new Bitmap(chestW, chestH, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(chestX, chestY, 0, 0, new Size(chestW, chestH), CopyPixelOperation.SourceCopy);
                }

                if (!CheckIsChestOpenFromBitmap(bmp))
                {
                    return results;
                }

                return ScanChestSlotsFromBitmap(bmp, rows: 5, cols: 5, candidateItems);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChestScanner ScanChestSlots Error]: {ex.Message}");
                return results;
            }
        }

        public List<ChestSlotResult> ScanChestSlotsFromBitmap(Bitmap bitmap, int rows = 4, int cols = 5, IEnumerable<Item>? candidateItems = null)
        {
            var results = new List<ChestSlotResult>();
            if (rows <= 0 || cols <= 0 || bitmap.Width <= 10 || bitmap.Height <= 10) return results;

            var itemsList = (candidateItems != null && candidateItems.Any())
                ? candidateItems.ToList()
                : ItemsRegistry.GetDefaultItems();

            double cellW = (double)bitmap.Width / cols;
            double cellH = (double)bitmap.Height / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int startX = (int)Math.Round(c * cellW);
                    int startY = (int)Math.Round(r * cellH);
                    int endX = (int)Math.Round((c + 1) * cellW);
                    int endY = (int)Math.Round((r + 1) * cellH);

                    int slotW = Math.Max(1, Math.Min(endX, bitmap.Width) - startX);
                    int slotH = Math.Max(1, Math.Min(endY, bitmap.Height) - startY);

                    var slotRect = new Rectangle(startX, startY, slotW, slotH);
                    using var slotBmp = bitmap.Clone(slotRect, bitmap.PixelFormat);

                    var slotResult = AnalyzeSingleSlot(slotBmp, r, c, itemsList);

                    if (slotResult.HasItem)
                    {
                        slotResult.Quantity = TryReadQuantity(slotBmp);
                    }

                    results.Add(slotResult);
                }
            }

            return results;
        }

        private ChestSlotResult AnalyzeSingleSlot(Bitmap slotBmp, int row, int col, List<Item> candidateItems)
        {
            var result = new ChestSlotResult { Row = row, Col = col, HasItem = false, Item = null, Quantity = 0 };

            int w = slotBmp.Width;
            int h = slotBmp.Height;

            int insetX = (int)(w * 0.15);
            int insetY = (int)(h * 0.15);
            int innerW = Math.Max(1, w - insetX * 2);
            int innerH = Math.Max(1, h - insetY * 2);

            int qtyStartX = (int)(w * 0.55);
            int qtyStartY = (int)(h * 0.55);

            BitmapData data = slotBmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] bytes = new byte[Math.Abs(data.Stride) * h];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            slotBmp.UnlockBits(data);

            var slotPixels = new List<Color>();

            for (int y = insetY; y < insetY + innerH; y++)
            {
                int rowOffset = y * data.Stride;
                for (int x = insetX; x < insetX + innerW; x++)
                {
                    if (x >= qtyStartX && y >= qtyStartY) continue;

                    int px = rowOffset + (x * 4);
                    byte b = bytes[px];
                    byte g = bytes[px + 1];
                    byte r = bytes[px + 2];

                    Color pxColor = Color.FromArgb(r, g, b);

                    if (!IsStrictChestBackground(pxColor))
                    {
                        slotPixels.Add(pxColor);
                    }
                }
            }

            if (slotPixels.Any())
            {
                // 1. LẤY RA TOP 15 MÀU XUẤT HIỆN NHIỀU NHẤT TRONG SLOT
                var slotTopColorsWithCount = slotPixels
                    .GroupBy(c => c)
                    .OrderByDescending(g => g.Count())
                    .Take(15)
                    .Select(g => new { Color = g.Key, Hex = ColorTranslator.ToHtml(g.Key), Count = g.Count() })
                    .ToList();

                // Log Top 15 Colors (Giữ nguyên 100%)
                Debug.WriteLine($"--- [Slot ({row + 1}x{col + 1})] TOP 15 COLORS ---");
                foreach (var colorItem in slotTopColorsWithCount)
                {
                    Debug.WriteLine($"  - Hex: {colorItem.Hex} | RGB: ({colorItem.Color.R}, {colorItem.Color.G}, {colorItem.Color.B}) | Total: {colorItem.Color.R + colorItem.Color.G + colorItem.Color.B}");
                }
                Debug.WriteLine("------------------------------------------");

                Item? bestItem = null;
                double maxMatchedScore = 0.0;

                foreach (var item in candidateItems)
                {
                    double currentMatchedScore = CalculateSlotScore(slotPixels, item);

                    if (currentMatchedScore <= 0)
                    {
                        continue;
                    }

                    if (currentMatchedScore > maxMatchedScore)
                    {
                        maxMatchedScore = currentMatchedScore;
                        bestItem = item;
                    }
                }

                if (bestItem != null && maxMatchedScore >= 500.0)
                {
                    result.HasItem = true;
                    result.Item = bestItem;
                    Debug.WriteLine($"[Slot ({row + 1}x{col + 1})] Selected: {bestItem.DisplayName} | Score: {maxMatchedScore:F1}");
                }
            }
            else
            {
                Debug.WriteLine("Not Found Item " + slotPixels.Count + "Count");
            }

            return result;
        }

        private int TryReadQuantity(Bitmap slotBmp)
        {
            if (_tesseractDetector == null || !_tesseractDetector.IsAvailable)
                return 1;

            try
            {
                int w = slotBmp.Width;
                int h = slotBmp.Height;

                int cropX = (int)(w * 0.58);
                int cropY = (int)(h * 0.58);
                int cropW = w - cropX;
                int cropH = h - cropY;

                var cropRect = new Rectangle(cropX, cropY, cropW, cropH);
                using var numBmp = slotBmp.Clone(cropRect, slotBmp.PixelFormat);

                using var processedBmp = PreprocessQuantityForOcr(numBmp);

                string text = _tesseractDetector.RecognizeText(processedBmp).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    string digits = new string(text.Where(char.IsDigit).ToArray());
                    if (int.TryParse(digits, out int qty) && qty >= 1 && qty <= 999)
                    {
                        return qty;
                    }
                }
            }
            catch
            {
                // Fallback nếu không đọc được
            }

            return 1;
        }

        private static Bitmap PreprocessQuantityForOcr(Bitmap src)
        {
            int w = src.Width * 3;
            int h = src.Height * 3;

            var dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, w, h);
            }

            var rect = new Rectangle(0, 0, w, h);
            BitmapData data = dst.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] bytes = new byte[Math.Abs(data.Stride) * h];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            for (int y = 0; y < h; y++)
            {
                int rowOffset = y * data.Stride;
                for (int x = 0; x < w; x++)
                {
                    int px = rowOffset + (x * 3);
                    byte b = bytes[px];
                    byte g = bytes[px + 1];
                    byte r = bytes[px + 2];

                    bool isWhiteDigit = (r >= 170 && g >= 170 && b >= 170);

                    if (isWhiteDigit)
                    {
                        bytes[px] = 0;
                        bytes[px + 1] = 0;
                        bytes[px + 2] = 0;
                    }
                    else
                    {
                        bytes[px] = 255;
                        bytes[px + 1] = 255;
                        bytes[px + 2] = 255;
                    }
                }
            }

            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            dst.UnlockBits(data);

            return dst;
        }

        private static double CalculateSlotScore(List<Color> slotPixels, Item item)
        {
            if (slotPixels == null || slotPixels.Count < 80) return 0.0;

            // 1. Náº¿u item cÃ³ hÃ m Ä‘Ã¡nh giÃ¡ Ä‘áº·c trÆ°ng riÃªng
            if (item.CustomEvaluator != null)
            {
                return item.CustomEvaluator(slotPixels);
            }

            // 2. Fallback so khá»›p theo danh sÃ¡ch Colors chuáº©n
            if (item.Colors != null && item.Colors.Count > 0)
            {
                double matched = 0.0;
                foreach (var px in slotPixels)
                {
                    if (item.Colors.Any(c => CalculateColorDistance(px, c) <= item.ColorTolerance))
                    {
                        matched++;
                    }
                }
                return matched;
            }

            return 0.0;
        }

        public static bool IsStrictChestBackground(Color c)
        {
            // Ná»n rÆ°Æ¡ng lÃ  mÃ u xÃ¡m trung tÃ­nh (RGB tá»« 40 Ä‘áº¿n 46, vÃ  Ä‘á»™ lá»‡ch giá»¯a cÃ¡c kÃªnh <= 2)
            if (c.R >= 40 && c.R <= 46 &&
                Math.Abs(c.R - c.G) <= 2 &&
                Math.Abs(c.G - c.B) <= 2 &&
                Math.Abs(c.R - c.B) <= 2)
            {
                return true;
            }

            // Danh sÃ¡ch cÃ¡c mÃ£ mÃ u ná»n cá»‘ Ä‘á»‹nh
            for (int i = 0; i < ChestSlotBackgroundColors.Count; i++)
            {
                var bg = ChestSlotBackgroundColors[i];
                if (c.R == bg.R && c.G == bg.G && c.B == bg.B)
                {
                    return true;
                }
            }

            return false;
        }
        public static double CalculateColorDistance(Color c1, Color c2)
        {
            int rDiff = c1.R - c2.R;
            int gDiff = c1.G - c2.G;
            int bDiff = c1.B - c2.B;
            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }
    }
}
