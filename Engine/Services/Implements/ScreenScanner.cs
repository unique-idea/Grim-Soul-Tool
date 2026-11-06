using Engine.Models;
using Engine.References.Models;
using Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Engine.Services.Implements
{
    public class ScreenScanner : IScreenScanner
    {
        /// <summary>
        /// Chụp màn hình vùng Overlay, quét các ô từ tâm ra ngoài theo hình xoắn ốc
        /// và lấy tối đa top N màu đại diện cho mỗi ô.
        /// Bỏ qua các ô thuộc vùng đỏ tâm nhân vật.
        /// </summary>
        public List<ItemScan> ScanGridSpiral(int screenX, int screenY, int width, int height, int gridN, int minRadius = 1)
        {
            var results = new List<ItemScan>();

            if (width <= 0 || height <= 0 || gridN <= 0) return results;

            using Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(screenX, screenY, 0, 0, new Size(width, height));
            }

            double cellW = (double)width / gridN;
            double cellH = (double)height / gridN;

            int centerRow = gridN / 2;
            int centerCol = gridN / 2;

            // Tính toán khoảng biên ô trung tâm cần bỏ qua (Đồng bộ với OverlayWindow)
            int centerBase = gridN / 2;
            int minCenterRow = centerBase - minRadius;
            int maxCenterRow = centerBase + minRadius - 1;
            int minCenterCol = centerBase - minRadius;
            int maxCenterCol = centerBase + minRadius - 1;

            int maxRadius = Math.Max(gridN - centerRow, gridN - centerCol);
            var visited = new bool[gridN, gridN];

            // Vòng lặp r quét từ minRadius ra ngoài
            for (int r = minRadius; r <= maxRadius; r++)
            {
                for (int row = centerRow - r; row <= centerRow + r; row++)
                {
                    for (int col = centerCol - r; col <= centerCol + r; col++)
                    {
                        // Kiểm tra ra ngoài biên lưới
                        if (row < 0 || row >= gridN || col < 0 || col >= gridN) continue;

                        // Bỏ qua nếu ô thuộc vùng đỏ tâm nhân vật
                        bool isCenterZone = (row >= minCenterRow && row <= maxCenterRow) &&
                                            (col >= minCenterCol && col <= maxCenterCol);
                        if (isCenterZone) continue;

                        int currentRadius = Math.Max(Math.Abs(row - centerRow), Math.Abs(col - centerCol));
                        if (currentRadius != r || visited[row, col]) continue;

                        visited[row, col] = true;

                        int startX = (int)(col * cellW);
                        int startY = (int)(row * cellH);
                        int cellWInt = Math.Max(1, (int)cellW);
                        int cellHInt = Math.Max(1, (int)cellH);

                        List<Color> topColors = ExtractTopColors(bitmap, startX, startY, cellWInt, cellHInt, 5);

                        results.Add(new ItemScan
                        {
                            Row = row,
                            Col = col,
                            Radius = r,
                            item = new Item { Colors = topColors }
                        });
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Lấy ra top N màu chủ đạo/đặc trưng nhất trong vùng ô vuông.
        /// </summary>
        private static List<Color> ExtractTopColors(Bitmap bmp, int startX, int startY, int w, int h, int topCount)
        {
            var colorCount = new Dictionary<int, int>();

            // Lấy mẫu (sample step) để tăng tốc độ xử lý ảnh
            int stepX = Math.Max(1, w / 10);
            int stepY = Math.Max(1, h / 10);

            for (int y = startY; y < startY + h && y < bmp.Height; y += stepY)
            {
                for (int x = startX; x < startX + w && x < bmp.Width; x += stepX)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    // Làm tròn màu nhẹ (Quantization) để nhóm các màu gần giống nhau
                    int quantizedRgb = ((pixel.R / 16) * 16) << 16 |
                                       ((pixel.G / 16) * 16) << 8 |
                                       ((pixel.B / 16) * 16);

                    if (colorCount.ContainsKey(quantizedRgb))
                        colorCount[quantizedRgb]++;
                    else
                        colorCount[quantizedRgb] = 1;
                }
            }

            // Lấy ra top N màu có tần suất cao nhất
            var sortedColors = colorCount
                .OrderByDescending(pair => pair.Value)
                .Take(topCount)
                .Select(pair => System.Drawing.ColorTranslator.FromHtml($"#{pair.Key:X6}"))
                .ToList();

            return sortedColors;
        }
    }
}