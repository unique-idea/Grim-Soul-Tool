using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Engine.Models;
using Engine.Services.Interfaces;

namespace Engine.Services.Implements
{
    public class HealthBarScanner : IHealthBarScanner, IDisposable
    {
        private readonly IAction _action;
        private readonly TesseractOcrDetector _tesseractDetector;

        private int _lastKnownMaxHp = 120;
        private int _lastKnownCurrentHp = 120;

        public HealthBarScanner(IAction? action = null)
        {
            _action = action ?? new Action();
            _tesseractDetector = new TesseractOcrDetector();
        }

        /// <summary>
        /// Quét 2 vùng riêng biệt từ màn hình: 
        /// 1 Vùng chỉ chứa Thanh Máu (Bar Region) & 1 Vùng chỉ chứa Số Máu Hiện Tại (Text Region).
        /// </summary>
        public async Task<PlayerHealthInfo> ScanSeparateRegionsFromScreenAsync(
            int barX, int barY, int barW, int barH,
            int textX, int textY, int textW, int textH,
            int maxHealth = 120,
            int healThresholdPercent = 40)
        {
            if (maxHealth <= 0) maxHealth = 120;
            var info = new PlayerHealthInfo { MaxHealth = maxHealth };

            if (barW <= 4 || barH <= 4 || textW <= 4 || textH <= 4)
                return info;

            return await Task.Run(() =>
            {
                // 1. Quét & đo riêng Vùng Thanh Máu (Bar Region)
                double visualBarPercent = 0.0;
                try
                {
                    using var barBmp = new Bitmap(barW, barH, PixelFormat.Format32bppArgb);
                    using (var gBar = Graphics.FromImage(barBmp))
                    {
                        gBar.CopyFromScreen(barX, barY, 0, 0, new Size(barW, barH), CopyPixelOperation.SourceCopy);
                    }
                    visualBarPercent = MeasureRedBarPercentageHSV(barBmp);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HealthBarScanner Bar Error]: {ex.Message}");
                }
                info.VisualBarPercentage = Math.Round(visualBarPercent, 1);

                // 2. Quét & nhận diện OCR riêng Vùng Số Máu (Text Region)
                int? currentHp = null;
                string detectionMethod = "None";
                string lastRawText = string.Empty;

                try
                {
                    using var textBmp = new Bitmap(textW, textH, PixelFormat.Format32bppArgb);
                    using (var gText = Graphics.FromImage(textBmp))
                    {
                        gText.CopyFromScreen(textX, textY, 0, 0, new Size(textW, textH), CopyPixelOperation.SourceCopy);
                    }

                    if (_tesseractDetector != null && _tesseractDetector.IsAvailable)
                    {
                        // Attempt 1: Lọc nền game thành Trắng-Đen chuẩn hóa (Binarized HP Text) phóng đại 2x
                        using (var bmp1 = PreprocessHpTextForOcr(textBmp, 2))
                        {
                            var (hp1, raw1) = _tesseractDetector.RecognizeCurrentHpOnly(bmp1, maxHealth);
                            lastRawText = raw1;
                            if (hp1.HasValue && hp1.Value >= 0 && hp1.Value <= maxHealth)
                            {
                                currentHp = hp1.Value;
                                detectionMethod = "Binarized2x";
                            }
                        }

                        // Attempt 2: Đọc trực tiếp phóng đại 3x mượt Bicubic
                        if (!currentHp.HasValue)
                        {
                            using (var bmp2 = ScaleBitmap(textBmp, 3))
                            {
                                var (hp2, raw2) = _tesseractDetector.RecognizeCurrentHpOnly(bmp2, maxHealth);
                                if (!string.IsNullOrEmpty(raw2)) lastRawText = raw2;
                                if (hp2.HasValue && hp2.Value >= 0 && hp2.Value <= maxHealth)
                                {
                                    currentHp = hp2.Value;
                                    detectionMethod = "Direct3x";
                                }
                            }
                        }

                        // Attempt 3: Tăng tương phản trực tiếp
                        if (!currentHp.HasValue)
                        {
                            using var bmp3 = EnhanceContrastDirect(textBmp);
                            var (hp3, raw3) = _tesseractDetector.RecognizeCurrentHpOnly(bmp3, maxHealth);
                            if (!string.IsNullOrEmpty(raw3)) lastRawText = raw3;
                            if (hp3.HasValue && hp3.Value >= 0 && hp3.Value <= maxHealth)
                            {
                                currentHp = hp3.Value;
                                detectionMethod = "ContrastEnhanced";
                            }
                        }

                        // Fallback: Ước lượng số gần đúng từ raw text nếu đọc bị thiếu nét
                        if (!currentHp.HasValue)
                        {
                            currentHp = TesseractOcrDetector.GuessNearestValidHp(lastRawText, maxHealth, visualBarPercent);
                            detectionMethod = "Guessed";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HealthBarScanner Text Error]: {ex.Message}");
                }

                info.OcrCurrentHp = currentHp ?? -1;
                info.DetectionMethod = detectionMethod;
                info.RawOcrText = lastRawText;

                // 3. Dung hòa dữ liệu thu thập (Multi-Sensor Fusion)
                double textPercent = (currentHp.HasValue && currentHp.Value >= 0)
                    ? Math.Round(((double)currentHp.Value / maxHealth) * 100.0, 1)
                    : -1.0;
                info.TextPercentage = textPercent;

                // Kiểm tra hợp lệ 2 phía:
                // Phía 1 (Text OCR): hợp lệ khi currentHp nằm trong khoảng [0, maxHealth]
                bool hasText = (currentHp.HasValue && currentHp.Value >= 0 && currentHp.Value <= maxHealth && textPercent >= 0.0 && textPercent <= 100.0);
                
                // Phía 2 (Bar Visual): hợp lệ khi visualBarPercent > 0 và <= 100
                bool hasBar = (visualBarPercent > 0.0 && visualBarPercent <= 100.0);

                if (hasText && hasBar)
                {
                    double diff = Math.Abs(textPercent - visualBarPercent);

                    if (diff >= 50.0)
                    {
                        // Tình huống 1: Hai bên quá lệch nhau (khoảng 50% như 23 vs 75) -> Lấy giá trị lớn hơn (75)
                        info.IsTextValid = (textPercent >= visualBarPercent);
                        info.FinalHealthPercentage = Math.Round(Math.Max(textPercent, visualBarPercent), 1);
                    }
                    else if (diff <= 10.0)
                    {
                        // Tình huống 2: Hai bên chênh lệch <= 10% -> Lấy trung bình cộng của cả hai
                        info.IsTextValid = true;
                        info.FinalHealthPercentage = Math.Round((textPercent + visualBarPercent) / 2.0, 1);
                    }
                    else
                    {
                        // Tình huống 3: Hai bên lệch từ 10% đến 50% -> Lấy giá trị nhỏ hơn (Lowest) để an toàn sinh tồn
                        info.IsTextValid = (textPercent <= visualBarPercent);
                        info.FinalHealthPercentage = Math.Round(Math.Min(textPercent, visualBarPercent), 1);
                    }

                    info.CurrentHealth = (int)Math.Round((info.FinalHealthPercentage / 100.0) * maxHealth);
                    _lastKnownMaxHp = maxHealth;
                    _lastKnownCurrentHp = info.CurrentHealth;
                }
                else if (hasBar && !hasText)
                {
                    // 1 bên không phát hiện được hoặc giá trị không hợp lệ (< 0 hoặc > maxHealth) -> Dùng bên Bar
                    info.IsTextValid = false;
                    info.FinalHealthPercentage = Math.Round(visualBarPercent, 1);
                    info.CurrentHealth = (int)Math.Round((visualBarPercent / 100.0) * maxHealth);
                    _lastKnownMaxHp = maxHealth;
                    _lastKnownCurrentHp = info.CurrentHealth;
                }
                else if (hasText && !hasBar)
                {
                    // 1 bên không phát hiện được -> Dùng bên Text
                    info.IsTextValid = true;
                    info.FinalHealthPercentage = textPercent;
                    info.CurrentHealth = currentHp!.Value;
                    _lastKnownMaxHp = maxHealth;
                    _lastKnownCurrentHp = info.CurrentHealth;
                }
                else
                {
                    // Cả 2 bên đều không phát hiện được
                    info.IsTextValid = false;
                    info.FinalHealthPercentage = 0.0;
                    info.CurrentHealth = 0;
                }

                // 4. Auto-Heal (khi HP <= healThresholdPercent)
                if (info.FinalHealthPercentage > 0 && info.FinalHealthPercentage <= healThresholdPercent)
                {
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            bool executed = _action.Heal();
                            if (executed)
                            {
                                info.HealTriggered = true;
                                Debug.WriteLine($"[HealthBarScanner] Auto-heal executed! (HP = {info.FinalHealthPercentage:F1}% <= {healThresholdPercent}%)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[HealthBarScanner Heal Error]: {ex.Message}");
                        }
                    });
                }

                return info;
            });
        }

        public async Task<PlayerHealthInfo> ScanFromScreenAsync(int screenX, int screenY, int width, int height, int maxHealth = 120, int barHeightPercent = 25)
        {
            int barH = (int)(height * (barHeightPercent / 100.0));
            int textH = height - barH;
            return await ScanSeparateRegionsFromScreenAsync(screenX, screenY, width, barH, screenX, screenY + barH, width, textH, maxHealth);
        }

        public Task<PlayerHealthInfo> ParseHealthBitmapAsync(Bitmap bitmap, int maxHealth = 120, int barHeightPercent = 25)
        {
            int barH = (int)(bitmap.Height * (barHeightPercent / 100.0));
            int textH = bitmap.Height - barH;

            using var barBmp = new Bitmap(bitmap.Width, barH, PixelFormat.Format32bppArgb);
            using (var gBar = Graphics.FromImage(barBmp))
                gBar.DrawImage(bitmap, 0, 0, new Rectangle(0, 0, bitmap.Width, barH), GraphicsUnit.Pixel);

            using var textBmp = new Bitmap(bitmap.Width, textH, PixelFormat.Format32bppArgb);
            using (var gText = Graphics.FromImage(textBmp))
                gText.DrawImage(bitmap, 0, 0, new Rectangle(0, barH, bitmap.Width, textH), GraphicsUnit.Pixel);

            double visualPercent = MeasureRedBarPercentageHSV(barBmp);
            var (hp, rawText) = _tesseractDetector.RecognizeCurrentHpOnly(textBmp, maxHealth);

            var info = new PlayerHealthInfo
            {
                MaxHealth = maxHealth,
                VisualBarPercentage = Math.Round(visualPercent, 1),
                OcrCurrentHp = hp ?? -1,
                RawOcrText = rawText ?? string.Empty
            };

            if (hp.HasValue && hp.Value >= 0)
            {
                info.CurrentHealth = hp.Value;
                info.FinalHealthPercentage = Math.Round(((double)hp.Value / maxHealth) * 100.0, 1);
            }
            else
            {
                info.FinalHealthPercentage = Math.Round(visualPercent, 1);
                info.CurrentHealth = (int)Math.Round((visualPercent / 100.0) * maxHealth);
            }

            return Task.FromResult(info);
        }

        /// <summary>
        /// Thuật toán đo % vệt màu đỏ thanh máu dựa trên diện tích pixel đỏ thực tế trên tổng chiều rộng barW.
        /// Không phụ thuộc vào dải màu xám hay nền game đằng sau.
        /// </summary>
        public static double MeasureRedBarPercentageHSV(Bitmap bitmap)
        {
            if (bitmap == null || bitmap.Width <= 4 || bitmap.Height <= 2)
                return 0.0;

            int width = bitmap.Width;
            int height = bitmap.Height;

            var rect = new Rectangle(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] bytes = new byte[Math.Abs(data.Stride) * height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);

            int totalRedPixels = 0;
            int rowsWithRed = 0;
            int maxRedRunInSingleRow = 0;

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * data.Stride;
                int rowRedCount = 0;

                for (int x = 0; x < width; x++)
                {
                    int px = rowOffset + (x * 4);
                    byte b = bytes[px];
                    byte g = bytes[px + 1];
                    byte r = bytes[px + 2];

                    if (IsRedHpBarColorHSV(r, g, b))
                    {
                        rowRedCount++;
                    }
                }

                if (rowRedCount > 0)
                {
                    totalRedPixels += rowRedCount;
                    rowsWithRed++;
                    if (rowRedCount > maxRedRunInSingleRow)
                    {
                        maxRedRunInSingleRow = rowRedCount;
                    }
                }
            }

            // Nếu không tìm thấy pixel đỏ nào -> 0% HP
            if (totalRedPixels == 0 || rowsWithRed == 0)
                return 0.0;

            // Chiều dài vệt đỏ thực tế (Trung bình số pixel đỏ trên mỗi hàng có chứa máu)
            double avgRedLength = (double)totalRedPixels / rowsWithRed;

            // Lấy giá trị lớn nhất giữa trung bình và vệt đỏ dài nhất hàng để bù xê dịch viền
            double finalRedWidth = Math.Max(avgRedLength, maxRedRunInSingleRow);

            // Tính tỷ lệ phần trăm trực tiếp dựa trên tổng bề rộng width (barW) của Region
            double percentage = (finalRedWidth / width) * 100.0;

            return Math.Clamp(percentage, 0.0, 100.0);
        }

        /// <summary>
        /// Tiền xử lý lọc màu riêng ô chứa chữ số HP: Giữ lại màu Đỏ San Hô của số, xóa 100% nền game thành màu trắng
        /// </summary>
        private static Bitmap PreprocessHpTextForOcr(Bitmap src, int factor)
        {
            int w = src.Width * factor;
            int h = src.Height * factor;

            var dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
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

                    // Nhận diện nét chữ màu Đỏ San Hô
                    bool isHpRedText = (r >= 135) && (r > g + 25) && (r > b + 25);

                    if (isHpRedText)
                    {
                        // Chữ -> ĐEN
                        bytes[px] = 0;
                        bytes[px + 1] = 0;
                        bytes[px + 2] = 0;
                    }
                    else
                    {
                        // Nền game -> TRẮNG TUYỀN
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

        private static bool IsRedHpBarColorHSV(byte r, byte g, byte b)
        {
            if (r < 115 || r <= g + 15 || r <= b + 15) return false;

            Color color = Color.FromArgb(r, g, b);
            float hue = color.GetHue();
            float saturation = color.GetSaturation();
            float brightness = color.GetBrightness();

            bool isRedHue = (hue >= 335 || hue <= 25);
            bool hasGoodSaturation = saturation >= 0.25f;
            bool hasGoodBrightness = brightness >= 0.18f && brightness <= 0.92f;

            return isRedHue && hasGoodSaturation && hasGoodBrightness;
        }

        private static bool IsBackgroundBarTrack(byte r, byte g, byte b)
        {
            return (r >= 20 && r <= 110 && g >= 20 && g <= 110 && b >= 20 && b <= 110 && Math.Abs(r - g) <= 20 && Math.Abs(g - b) <= 20);
        }

        private static Bitmap ScaleBitmap(Bitmap src, int factor)
        {
            var dst = new Bitmap(src.Width * factor, src.Height * factor, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(src, 0, 0, dst.Width, dst.Height);
            }
            return dst;
        }

        private static Bitmap EnhanceContrastDirect(Bitmap src)
        {
            int w = src.Width * 2;
            int h = src.Height * 2;
            var dst = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(dst))
            {
                using var ia = new ImageAttributes();
                float c = 1.35f;
                float t = (1.0f - c) / 2.0f;
                ColorMatrix cm = new ColorMatrix(new float[][]
                {
                    new float[] {c, 0, 0, 0, 0},
                    new float[] {0, c, 0, 0, 0},
                    new float[] {0, 0, c, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {t, t, t, 0, 1}
                });
                ia.SetColorMatrix(cm);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, new Rectangle(0, 0, w, h), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
            }
            return dst;
        }

        public void Dispose()
        {
            _tesseractDetector?.Dispose();
        }
    }
}