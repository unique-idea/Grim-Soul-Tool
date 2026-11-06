using Engine.Enums;
using Engine.Models;
using Engine.References.Models;
using Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Engine.Services.Implements
{
    public class MiniMapScanner : IMiniMapScanner
    {
        private const double COLOR_TOLERANCE = 24.0;
        private const double COLOR_TOLERANCE_SQ = COLOR_TOLERANCE * COLOR_TOLERANCE; // 576.0
        private const int CELL_SIZE = 12; // pixels
        private const int MIN_CLUSTER_PIXELS = 4;

        public async Task<List<DetectedMapObject>> ScanObjectsFromScreenAsync(int screenX, int screenY, int width, int height, List<Item>? targets = null)
        {
            if (width <= 10 || height <= 10)
                return new List<DetectedMapObject>();

            return await Task.Run(() => ScanObjectsFromScreen(screenX, screenY, width, height, targets));
        }

        public List<DetectedMapObject> ScanObjectsFromScreen(int screenX, int screenY, int width, int height, List<Item>? targets = null)
        {
            if (width <= 10 || height <= 10)
                return new List<DetectedMapObject>();

            using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(screenX, screenY, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }

            return ScanObjects(bmp, targets);
        }

        public List<DetectedMapObject> ScanObjects(Bitmap bitmap, List<Item>? targets = null)
        {
            var results = new List<DetectedMapObject>();
            if (bitmap == null || bitmap.Width <= 10 || bitmap.Height <= 10)
                return results;

            targets ??= MapObjectRegistry.GetDefaultMapObjects();
            int width = bitmap.Width;
            int height = bitmap.Height;
            int centerX = width / 2;
            int centerY = height / 2;

            // Bỏ qua vùng tâm xung quanh nhân vật (tránh match phải mũi tên người chơi)
            int ignoreRadius = Math.Max(8, Math.Min(width, height) / 14);
            int ignoreRadiusSq = ignoreRadius * ignoreRadius;

            var rect = new Rectangle(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] bytes = new byte[Math.Abs(data.Stride) * height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);

            int gridW = (width + CELL_SIZE - 1) / CELL_SIZE;
            int gridH = (height + CELL_SIZE - 1) / CELL_SIZE;

            foreach (var target in targets)
            {
                if (target.Colors == null || !target.Colors.Any())
                    continue;

                // Gom pixel vào lưới ô (Spatial Grid Binning) để đạt hiệu năng O(N) cực nhanh
                int[,] cellCount = new int[gridW, gridH];
                long[,] cellSumX = new long[gridW, gridH];
                long[,] cellSumY = new long[gridW, gridH];

                for (int y = 0; y < height; y++)
                {
                    int rowOffset = y * data.Stride;
                    int dy = y - centerY;
                    int dySq = dy * dy;
                    int cellY = y / CELL_SIZE;

                    for (int x = 0; x < width; x++)
                    {
                        int dx = x - centerX;
                        if (dx * dx + dySq < ignoreRadiusSq)
                            continue;

                        int px = rowOffset + (x * 4);
                        byte b = bytes[px];
                        byte g = bytes[px + 1];
                        byte r = bytes[px + 2];

                        foreach (var sampleColor in target.Colors)
                        {
                            int rDiff = r - sampleColor.R;
                            int gDiff = g - sampleColor.G;
                            int bDiff = b - sampleColor.B;

                            if (rDiff * rDiff + gDiff * gDiff + bDiff * bDiff <= COLOR_TOLERANCE_SQ)
                            {
                                int cellX = x / CELL_SIZE;
                                cellCount[cellX, cellY]++;
                                cellSumX[cellX, cellY] += x;
                                cellSumY[cellX, cellY] += y;
                                break;
                            }
                        }
                    }
                }

                // Kết nối các ô lân cận thành cụm icon vật thể
                bool[,] visitedCell = new bool[gridW, gridH];

                for (int gx = 0; gx < gridW; gx++)
                {
                    for (int gy = 0; gy < gridH; gy++)
                    {
                        if (visitedCell[gx, gy] || cellCount[gx, gy] < 2)
                            continue;

                        int clusterCount = 0;
                        long clusterSumX = 0;
                        long clusterSumY = 0;

                        var queue = new Queue<(int x, int y)>();
                        queue.Enqueue((gx, gy));
                        visitedCell[gx, gy] = true;

                        while (queue.Count > 0)
                        {
                            var (cx, cy) = queue.Dequeue();
                            clusterCount += cellCount[cx, cy];
                            clusterSumX += cellSumX[cx, cy];
                            clusterSumY += cellSumY[cx, cy];

                            for (int ndx = -1; ndx <= 1; ndx++)
                            {
                                for (int ndy = -1; ndy <= 1; ndy++)
                                {
                                    int nx = cx + ndx;
                                    int ny = cy + ndy;
                                    if (nx >= 0 && nx < gridW && ny >= 0 && ny < gridH &&
                                        !visitedCell[nx, ny] && cellCount[nx, ny] >= 2)
                                    {
                                        visitedCell[nx, ny] = true;
                                        queue.Enqueue((nx, ny));
                                    }
                                }
                            }
                        }

                        if (clusterCount >= MIN_CLUSTER_PIXELS)
                        {
                            int avgX = (int)(clusterSumX / clusterCount);
                            int avgY = (int)(clusterSumY / clusterCount);
                            double dx = avgX - centerX;
                            double dy = avgY - centerY;
                            double dist = Math.Sqrt(dx * dx + dy * dy);

                            results.Add(new DetectedMapObject
                            {
                                Name = target.Name,
                                DisplayName = target.DisplayName,
                                Center = new Point(avgX, avgY),
                                DistanceFromCenter = Math.Round(dist, 1),
                                DeltaX = dx,
                                DeltaY = dy,
                                PixelMatchCount = clusterCount
                            });
                        }
                    }
                }
            }

            return results.OrderBy(o => o.DistanceFromCenter).ToList();
        }
    }
}
