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

        // 1. Màu xám chuẩn của lòng ô rương UI (Chính xác từ ảnh UI rương)
        public static readonly IReadOnlyList<Color> ChestSlotBackgroundColors = new List<Color>
        {
            ColorTranslator.FromHtml("#2A2928"),
            ColorTranslator.FromHtml("#2B2B2B"),
            ColorTranslator.FromHtml("#2B2C2B"),
            ColorTranslator.FromHtml("#383837"),
            ColorTranslator.FromHtml("#434443")
        };

        // 2. Màu đường viền lưới nâu sáng phân ô Matrix UI
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

                    // A. KIỂM TRA MÀU ĐÊM (NIGHT TINT): Ban đêm kênh B (Blue) chênh lệch cao hơn hẳn R và G
                    if (px.B > px.R + 8 && px.B > px.G + 6)
                    {
                        nightTintMatches++;
                    }

                    // B. KIỂM TRA MÀU NỀN UI RƯƠNG THUẦN (Xám trung tính, R, G, B lệch <= 6)
                    if (IsColorMatch(px, ChestSlotBackgroundColors, 14.0))
                    {
                        int maxDiff = Math.Max(Math.Abs(px.R - px.G), Math.Max(Math.Abs(px.G - px.B), Math.Abs(px.R - px.B)));
                        if (maxDiff <= 6)
                        {
                            validChestBgMatches++;
                        }
                    }

                    // C. KIỂM TRA MÀU ĐƯỜNG KẺ NÂU SÁNG MATRIX
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

            // 1. ĐIỀU KIỆN PHỦ QUYẾT: Nếu có sắc xanh đen đêm chiếm > 12% -> CHẮC CHẮN NỀN GAME (BÁO CLOSED)
            if (nightRatio >= 0.12)
            {
                Debug.WriteLine($"[Chest Check] Rejected: Night Blue Tint Detected ({nightRatio:P1})");
                return false;
            }

            // 2. XÁC NHẬN RƯƠNG MỞ: Màn hình phải có màu nền xám chuẩn >= 20% VÀ có vệt màu nâu sáng của lưới matrix
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

        public List<ChestSlotResult> ScanChestSlotsFromBitmap(Bitmap bitmap, int rows = 5, int cols = 5, IEnumerable<Item>? candidateItems = null)
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

        private class ColorCluster
        {
            public Color BaseColor { get; set; }
            public int PixelCount { get; set; }
        }

        private ChestSlotResult AnalyzeSingleSlot(Bitmap slotBmp, int row, int col, List<Item> candidateItems)
        {
            var result = new ChestSlotResult { Row = row, Col = col, HasItem = false, Item = null, Quantity = 0 };

            int w = slotBmp.Width;
            int h = slotBmp.Height;

            int insetX = (int)(w * 0.12);
            int insetY = (int)(h * 0.12);
            int innerW = Math.Max(1, w - insetX * 2);
            int innerH = Math.Max(1, h - insetY * 2);

            BitmapData data = slotBmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] bytes = new byte[Math.Abs(data.Stride) * h];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            slotBmp.UnlockBits(data);

            int totalInnerPixels = 0;
            int nonBgPixels = 0;

            var slotPixels = new List<Color>();

            for (int y = insetY; y < insetY + innerH; y++)
            {
                int rowOffset = y * data.Stride;
                for (int x = insetX; x < insetX + innerW; x++)
                {
                    int px = rowOffset + (x * 4);
                    byte b = bytes[px];
                    byte g = bytes[px + 1];
                    byte r = bytes[px + 2];

                    Color pxColor = Color.FromArgb(r, g, b);
                    totalInnerPixels++;

                    if (!IsSlotBackgroundColor(pxColor, 18.0))
                    {
                        nonBgPixels++;
                        slotPixels.Add(pxColor);
                    }
                }
            }

            double nonBgRatio = totalInnerPixels > 0 ? (double)nonBgPixels / totalInnerPixels : 0;

            // Ngưỡng kiểm tra ô có đồ hay không
            if (nonBgRatio >= 0.12 && slotPixels.Any())
            {
                Item? bestItem = null;
                double maxMatchRatio = 0.0;
                int maxMatchedPixelCount = 0;

                foreach (var item in candidateItems)
                {
                    if (item.Colors == null || !item.Colors.Any()) continue;

                    // Đếm số pixel trong slot khớp với bảng màu của Item này
                    int matchedPixels = 0;
                    var matchedRegistryColors = new HashSet<Color>();

                    // LẤY DUNG SAI TỰ ĐỊNH NGHĨA TỪ ITEM (HOẶC MẶC ĐỊNH 18.0)
                    double tolerance = item.ColorTolerance > 0 ? item.ColorTolerance : 18.0;

                    foreach (var pxColor in slotPixels)
                    {
                        foreach (var itemColor in item.Colors)
                        {
                            // SO SÁNH THEO TỪNG COLOR TOLERANCE ĐỘNG CỦA ITEM
                            if (CalculateColorDistance(pxColor, itemColor) <= tolerance)
                            {
                                matchedPixels++;
                                matchedRegistryColors.Add(itemColor);
                                break;
                            }
                        }
                    }

                    // Tỉ lệ % màu trong Registry của Item được tìm thấy trên ảnh
                    double registryCoverage = (double)matchedRegistryColors.Count / item.Colors.Count;
                    // Tỉ lệ % Pixel trên ảnh thuộc về Item này
                    double pixelCoverage = (double)matchedPixels / slotPixels.Count;

                    // Điểm số kết hợp: Ưu tiên registryCoverage cao & pixelCoverage cao
                    double totalScore = (registryCoverage * 0.6) + (pixelCoverage * 0.4);

                    // Điều kiện chọn Item: Đạt tối thiểu 25% màu registry và 15px
                    if (registryCoverage >= 0.25 && matchedPixels >= 15)
                    {
                        if (totalScore > maxMatchRatio)
                        {
                            maxMatchRatio = totalScore;
                            maxMatchedPixelCount = matchedPixels;
                            bestItem = item;
                        }
                    }
                }

                if (bestItem != null)
                {
                    result.HasItem = true;
                    result.Item = bestItem;
                    Debug.WriteLine($"[Slot ({row + 1}x{col + 1})] Selected: {bestItem.DisplayName} | Score: {maxMatchRatio:P1} ({maxMatchedPixelCount}px)");
                }
                else
                {
                    Debug.WriteLine($"[Slot ({row + 1}x{col + 1})] UNKNOWN ITEM | NonBg Ratio: {nonBgRatio:P1}");
                }
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

        public static bool IsSlotBackgroundColor(Color c, double tolerance = 18.0)
        {
            foreach (var bg in ChestSlotBackgroundColors)
            {
                if (CalculateColorDistance(c, bg) <= tolerance)
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