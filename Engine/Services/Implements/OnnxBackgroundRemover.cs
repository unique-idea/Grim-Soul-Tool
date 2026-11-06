using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Services.Implements
{
    public class OnnxBackgroundRemover : IDisposable
    {
        private InferenceSession? _session;
        private readonly object _lock = new();
        private bool _isInitialized;
        private readonly string? _modelPath;

        public bool IsAvailable => _isInitialized && _session != null;

        public OnnxBackgroundRemover()
        {
            _modelPath = FindModelPath();
            InitializeSession();
        }

        private static string? FindModelPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = new[]
            {
                Path.Combine(baseDir, "u2net.onnx"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "Engine", "u2net.onnx")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Engine", "u2net.onnx")),
                @"E:\GrimSoulTool\Engine\u2net.onnx"
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        private void InitializeSession()
        {
            if (string.IsNullOrEmpty(_modelPath) || !File.Exists(_modelPath))
            {
                Debug.WriteLine("[OnnxBackgroundRemover] Model file 'u2net.onnx' not found.");
                return;
            }

            try
            {
                var options = new SessionOptions();
                options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                _session = new InferenceSession(_modelPath, options);
                _isInitialized = true;
                Debug.WriteLine($"[OnnxBackgroundRemover] Loaded ONNX model from: {_modelPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnnxBackgroundRemover Init Error]: {ex.Message}");
                _session = null;
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Removes the game background from the bitmap using U2-Net ONNX model and ImageSharp,
        /// isolating text and foreground elements with high contrast.
        /// </summary>
        public System.Drawing.Bitmap RemoveBackground(System.Drawing.Bitmap srcBitmap)
        {
            if (srcBitmap == null || srcBitmap.Width <= 5 || srcBitmap.Height <= 5)
                return (System.Drawing.Bitmap)srcBitmap?.Clone()!;

            if (!IsAvailable)
            {
                // Fall back to rule-based segmentation if ONNX session is not active
                return FallbackColorRemoveBackground(srcBitmap);
            }

            lock (_lock)
            {
                try
                {
                    int origW = srcBitmap.Width;
                    int origH = srcBitmap.Height;

                    // 1. Convert System.Drawing.Bitmap to SixLabors.ImageSharp Image<Rgb24>
                    using var ms = new MemoryStream();
                    srcBitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    using var image = SixLabors.ImageSharp.Image.Load<Rgb24>(ms);

                    // 2. Resize to 320x320 for U2-Net input
                    const int targetSize = 320;
                    using var resized = image.Clone(ctx => ctx.Resize(targetSize, targetSize));

                    // 3. Build DenseTensor<float> normalized with ImageNet stats
                    var tensor = new DenseTensor<float>(new[] { 1, 3, targetSize, targetSize });
                    float[] mean = { 0.485f, 0.456f, 0.406f };
                    float[] std = { 0.229f, 0.224f, 0.225f };

                    resized.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < targetSize; y++)
                        {
                            var pixelSpan = accessor.GetRowSpan(y);
                            for (int x = 0; x < targetSize; x++)
                            {
                                var p = pixelSpan[x];
                                tensor[0, 0, y, x] = ((p.R / 255.0f) - mean[0]) / std[0];
                                tensor[0, 1, y, x] = ((p.G / 255.0f) - mean[1]) / std[1];
                                tensor[0, 2, y, x] = ((p.B / 255.0f) - mean[2]) / std[2];
                            }
                        }
                    });

                    // 4. Run ONNX Inference
                    string inputName = _session!.InputMetadata.Keys.First();
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor(inputName, tensor)
                    };

                    using var results = _session.Run(inputs);
                    // Output is typically "d0" (the fused output saliency map)
                    var output = results.First();
                    var outputTensor = output.AsTensor<float>();

                    // 5. Min-Max normalize the saliency map to [0, 1]
                    float minVal = float.MaxValue;
                    float maxVal = float.MinValue;

                    for (int y = 0; y < targetSize; y++)
                    {
                        for (int x = 0; x < targetSize; x++)
                        {
                            float v = outputTensor[0, 0, y, x];
                            if (v < minVal) minVal = v;
                            if (v > maxVal) maxVal = v;
                        }
                    }

                    float range = maxVal - minVal;
                    if (range < 1e-5f) range = 1.0f;

                    // 6. Create destination bitmap applying mask + text preservation
                    var dst = new System.Drawing.Bitmap(origW, origH, PixelFormat.Format32bppArgb);
                    var rect = new System.Drawing.Rectangle(0, 0, origW, origH);
                    BitmapData srcData = srcBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    BitmapData dstData = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                    try
                    {
                        int stride = srcData.Stride;
                        byte[] srcBytes = new byte[Math.Abs(stride) * origH];
                        byte[] dstBytes = new byte[Math.Abs(stride) * origH];
                        Marshal.Copy(srcData.Scan0, srcBytes, 0, srcBytes.Length);

                        for (int y = 0; y < origH; y++)
                        {
                            int rowOffset = y * stride;
                            int maskY = Math.Clamp((int)(y * (float)targetSize / origH), 0, targetSize - 1);

                            for (int x = 0; x < origW; x++)
                            {
                                int px = rowOffset + (x * 4);
                                byte b = srcBytes[px];
                                byte g = srcBytes[px + 1];
                                byte r = srcBytes[px + 2];

                                int maskX = Math.Clamp((int)(x * (float)targetSize / origW), 0, targetSize - 1);
                                float rawMask = (outputTensor[0, 0, maskY, maskX] - minVal) / range;

                                // Text color detection safeguards text from being masked out
                                int lum = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                                bool isWhiteText = (lum >= 115) && (Math.Abs(r - g) <= 35) && (Math.Abs(g - b) <= 35);
                                bool isHpNumber = (r >= 120) && (g >= 55) && (b >= 55) && (r > g + 15);

                                if (isWhiteText || isHpNumber || rawMask >= 0.40f)
                                {
                                    // Foreground text / salient object: boost contrast for OCR
                                    dstBytes[px] = (byte)Math.Min(255, (int)(b * 1.25));
                                    dstBytes[px + 1] = (byte)Math.Min(255, (int)(g * 1.25));
                                    dstBytes[px + 2] = (byte)Math.Min(255, (int)(r * 1.25));
                                    dstBytes[px + 3] = 255;
                                }
                                else
                                {
                                    // Background: clean uniform dark
                                    dstBytes[px] = 20;
                                    dstBytes[px + 1] = 20;
                                    dstBytes[px + 2] = 20;
                                    dstBytes[px + 3] = 255;
                                }
                            }
                        }

                        Marshal.Copy(dstBytes, 0, dstData.Scan0, dstBytes.Length);
                    }
                    finally
                    {
                        srcBitmap.UnlockBits(srcData);
                        dst.UnlockBits(dstData);
                    }

                    return dst;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[OnnxBackgroundRemover Error]: {ex.Message}");
                    return FallbackColorRemoveBackground(srcBitmap);
                }
            }
        }

        private static System.Drawing.Bitmap FallbackColorRemoveBackground(System.Drawing.Bitmap src)
        {
            int width = src.Width;
            int height = src.Height;
            var dst = new System.Drawing.Bitmap(width, height, PixelFormat.Format32bppArgb);

            var rect = new System.Drawing.Rectangle(0, 0, width, height);
            BitmapData srcData = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                int stride = srcData.Stride;
                byte[] srcBytes = new byte[Math.Abs(stride) * height];
                byte[] dstBytes = new byte[Math.Abs(stride) * height];
                Marshal.Copy(srcData.Scan0, srcBytes, 0, srcBytes.Length);

                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        int px = rowOffset + (x * 4);
                        byte b = srcBytes[px];
                        byte g = srcBytes[px + 1];
                        byte r = srcBytes[px + 2];

                        int lum = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                        bool isWhite = (lum >= 115) && (Math.Abs(r - g) <= 35) && (Math.Abs(g - b) <= 35);
                        bool isHpNumber = (r >= 120) && (g >= 55) && (b >= 55) && (r > g + 15);

                        if (isWhite || isHpNumber)
                        {
                            dstBytes[px] = (byte)Math.Min(255, (int)(b * 1.25));
                            dstBytes[px + 1] = (byte)Math.Min(255, (int)(g * 1.25));
                            dstBytes[px + 2] = (byte)Math.Min(255, (int)(r * 1.25));
                            dstBytes[px + 3] = 255;
                        }
                        else
                        {
                            dstBytes[px] = 20;
                            dstBytes[px + 1] = 20;
                            dstBytes[px + 2] = 20;
                            dstBytes[px + 3] = 255;
                        }
                    }
                }

                Marshal.Copy(dstBytes, 0, dstData.Scan0, dstBytes.Length);
            }
            finally
            {
                src.UnlockBits(srcData);
                dst.UnlockBits(dstData);
            }

            return dst;
        }

        public void Dispose()
        {
            _session?.Dispose();
            _session = null;
            _isInitialized = false;
        }
    }
}
