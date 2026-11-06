using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Tesseract;

namespace Engine.Services.Implements
{
    public class TesseractOcrDetector : IDisposable
    {
        private TesseractEngine? _engine;
        private readonly object _lock = new();
        private bool _isInitialized;
        private readonly string? _tessdataPath;

        public bool IsAvailable => _isInitialized && _engine != null;

        public TesseractOcrDetector()
        {
            _tessdataPath = FindTessdataPath();
            InitializeEngine();
        }

        private static string? FindTessdataPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = new[]
            {
                Path.Combine(baseDir, "tessdata"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "Engine", "tessdata")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Engine", "tessdata")),
                @"E:\GrimSoulTool\Engine\tessdata"
            };

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "eng.traineddata")))
                {
                    return candidate;
                }
            }

            return null;
        }

        private void InitializeEngine()
        {
            if (string.IsNullOrEmpty(_tessdataPath))
            {
                Debug.WriteLine("[TesseractOcrDetector] tessdata directory containing eng.traineddata not found.");
                return;
            }

            try
            {
                _engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
                _engine.SetVariable("tessedit_char_whitelist", "0123456789/abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ +-:()[]_.");
                _isInitialized = true;
                Debug.WriteLine($"[TesseractOcrDetector] Initialized TesseractEngine from: {_tessdataPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TesseractOcrDetector Init Error]: {ex.Message}");
                _engine = null;
                _isInitialized = false;
            }
        }

        public (int? CurrentHp, string RawText) RecognizeCurrentHpOnly(Bitmap bitmap, int maxHealth)
        {
            if (!IsAvailable || bitmap == null)
                return (null, string.Empty);

            lock (_lock)
            {
                try
                {
                    using var ms = new MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] pngBytes = ms.ToArray();

                    using var pix = Pix.LoadFromMemory(pngBytes);
                    using var page = _engine!.Process(pix);
                    string rawText = page.GetText() ?? string.Empty;

                    int? hp = ExtractCurrentHp(rawText, maxHealth);
                    return (hp, rawText.Trim());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TesseractOcrDetector Process Error]: {ex.Message}");
                    return (null, string.Empty);
                }
            }
        }

        public static int? ExtractCurrentHp(string rawText, int maxHealth)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return null;

            // 1. Primary HP match: "90/102", "90 / 100", "90|100", "90-100", "90)100"
            var slashMatch = Regex.Match(rawText, @"(\d{1,4})\s*[/\\|\)\]\}IltT:\-]\s*(\d{1,4})");
            if (slashMatch.Success && int.TryParse(slashMatch.Groups[1].Value, out int slashCur))
            {
                if (slashCur >= 0 && slashCur <= maxHealth)
                    return slashCur;
            }

            // 2. Space separated: "90 100"
            var spaceMatch = Regex.Match(rawText, @"(\d{1,4})\s+(\d{1,4})");
            if (spaceMatch.Success && int.TryParse(spaceMatch.Groups[1].Value, out int spaceCur))
            {
                if (spaceCur >= 0 && spaceCur <= maxHealth)
                    return spaceCur;
            }

            // 3. Standalone valid numbers 0 <= hp <= maxHealth
            var matches = Regex.Matches(rawText, @"\b(\d{1,4})\b");
            foreach (Match m in matches)
            {
                if (int.TryParse(m.Groups[1].Value, out int val) && val >= 0 && val <= maxHealth)
                {
                    return val;
                }
            }

            return null; // Violation (negative, > maxHealth, or unparsed)
        }

        public static int GuessNearestValidHp(string rawText, int maxHealth, double visualBarPercent)
        {
            double targetHp = Math.Clamp((visualBarPercent / 100.0) * maxHealth, 0, maxHealth);

            if (string.IsNullOrWhiteSpace(rawText))
                return (int)Math.Round(targetHp);

            var digitMatches = Regex.Matches(rawText, @"\d+");
            var candidates = new HashSet<int>();

            foreach (Match m in digitMatches)
            {
                string s = m.Value;
                for (int len = 1; len <= s.Length; len++)
                {
                    for (int start = 0; start <= s.Length - len; start++)
                    {
                        string sub = s.Substring(start, len);
                        if (int.TryParse(sub, out int val) && val >= 0 && val <= maxHealth)
                        {
                            candidates.Add(val);
                        }
                    }
                }
            }

            if (candidates.Count > 0)
            {
                int best = (int)Math.Round(targetHp);
                double minDiff = double.MaxValue;
                foreach (int c in candidates)
                {
                    double diff = Math.Abs(c - targetHp);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        best = c;
                    }
                }
                return best;
            }

            return (int)Math.Round(targetHp);
        }

        public (int CurrentHp, int MaxHp, string RawText) RecognizeHp(Bitmap bitmap)
        {
            var result = Recognize(bitmap);
            return (result.CurrentHp, result.MaxHp, result.RawText);
        }

        public (string Name, int CurrentHp, int MaxHp, int Level, string RawText) Recognize(Bitmap bitmap)
        {
            if (!IsAvailable || bitmap == null)
                return (string.Empty, 0, 0, 0, string.Empty);

            lock (_lock)
            {
                try
                {
                    using var ms = new MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] pngBytes = ms.ToArray();

                    using var pix = Pix.LoadFromMemory(pngBytes);
                    using var page = _engine!.Process(pix);
                    string rawText = page.GetText() ?? string.Empty;

                    return ParseRecognizedText(rawText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TesseractOcrDetector Process Error]: {ex.Message}");
                    return (string.Empty, 0, 0, 0, string.Empty);
                }
            }
        }

        public static (string Name, int CurrentHp, int MaxHp, int Level, string RawText) ParseRecognizedText(string rawText)
        {
            string name = string.Empty;
            int currentHp = 0;
            int maxHp = 0;
            int level = 0;

            if (string.IsNullOrWhiteSpace(rawText))
                return (name, currentHp, maxHp, level, string.Empty);

            // 1. Primary HP match across entire text: "90/102", "90 / 102", "91) 102", "90|102", "90-102"
            var hpMatch = Regex.Match(rawText, @"(\d{1,4})\s*[/\\|\)\]\}IltT:\-]\s*(\d{1,4})");
            if (hpMatch.Success)
            {
                if (int.TryParse(hpMatch.Groups[1].Value, out int c) && int.TryParse(hpMatch.Groups[2].Value, out int m))
                {
                    currentHp = c;
                    maxHp = m;
                }
            }
            else
            {
                // Fallback: Two separated numbers "90 102"
                var hpSpaceMatch = Regex.Match(rawText, @"(\d{1,4})\s+(\d{2,4})");
                if (hpSpaceMatch.Success)
                {
                    if (int.TryParse(hpSpaceMatch.Groups[1].Value, out int c) && int.TryParse(hpSpaceMatch.Groups[2].Value, out int m))
                    {
                        currentHp = c;
                        maxHp = m;
                    }
                }
            }

            return (name, currentHp, maxHp, level, rawText.Trim());
        }

        public string RecognizeText(Bitmap bitmap)
        {
            if (!IsAvailable || bitmap == null)
                return string.Empty;

            lock (_lock)
            {
                try
                {
                    using var ms = new MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] pngBytes = ms.ToArray();

                    using var pix = Pix.LoadFromMemory(pngBytes);
                    using var page = _engine!.Process(pix);
                    return (page.GetText() ?? string.Empty).Trim();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TesseractOcrDetector RecognizeText Error]: {ex.Message}");
                    return string.Empty;
                }
            }
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _engine = null;
            _isInitialized = false;
        }
    }
}
