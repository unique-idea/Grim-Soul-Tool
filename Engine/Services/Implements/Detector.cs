using Engine.Enums;
using Engine.Models;
using Engine.References.Models;
using Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Engine.Services.Implements
{
    public class Detector : IDetector
    {
        private readonly Item _defaultItem = new Item
        {
           Name = ItemName.Unknown,
        };
        // Danh sách các Item đã định nghĩa màu mẫu trong hệ thống
        private readonly List<Item> _definedItems;

        // Ngưỡng khoảng cách màu tối đa để tính là "Màu giống nhau" (0 - 255)
        private const double COLOR_TOLERANCE = 35.0;

        // Ngưỡng % tối thiểu để xác nhận là đúng Item (ví dụ 50%)
        private const double MATCH_THRESHOLD = 0.50;

        public Detector() : this(ItemsRegistry.GetDefaultItems())
        {
        }

        public Detector(List<Item> definedItems)
        {
            _definedItems = definedItems ?? ItemsRegistry.GetDefaultItems();
        }

        /// <summary>
        /// Nhận diện Item dựa trên danh sách các màu quét được trong 1 ô.
        /// </summary>
        /// <param name="scannedColors">Danh sách 5 - 10 màu lấy từ ô quét</param>
        /// <returns>Trả về Item matched có độ tin cậy cao nhất, hoặc null nếu không match</returns>
        public Item DetectItemByColors(List<Color> scannedColors)
        {
            if (scannedColors == null || !scannedColors.Any())
                return _defaultItem;

            Item bestMatchItem = _defaultItem;
            double highestMatchRatio = 0.0;

            foreach (var item in _definedItems)
            {
                // Nếu Item không khai báo danh sách màu mẫu -> Bỏ qua
                if (item.Colors == null || !item.Colors.Any())
                    continue;

                int matchedCount = 0;

                // Kiểm tra xem có bao nhiêu màu trong ô quét tiệm cận với màu của Item mẫu
                foreach (var sampleColor in item.Colors)
                {
                    bool isMatched = scannedColors.Any(scannedColor =>
                        CalculateColorDistance(scannedColor, sampleColor) <= COLOR_TOLERANCE);

                    if (isMatched)
                    {
                        matchedCount++;
                    }
                }

                // Tính % độ tương đồng (Match Ratio)
                double matchRatio = (double)matchedCount / item.Colors.Count;

                // Chọn Item có % tương đồng cao nhất và vượt ngưỡng THRESHOLD
                if (matchRatio >= MATCH_THRESHOLD && matchRatio > highestMatchRatio)
                {
                    highestMatchRatio = matchRatio;
                    bestMatchItem = item;
                }
            }

            return bestMatchItem;
        }

        /// <summary>
        /// Tính khoảng cách Euclidean giữa 2 màu trong không gian RGB.
        /// </summary>
        private double CalculateColorDistance(Color c1, Color c2)
        {
            int rDiff = c1.R - c2.R;
            int gDiff = c1.G - c2.G;
            int bDiff = c1.B - c2.B;

            // Công thức Euclidean Distance 3D
            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }

        // Interface method cũ (Match 1 màu đơn lẻ)
        public Item DetectItemByColor(Color color)
        {
            return DetectItemByColors(new List<Color> { color });
        }

        public int DectectItemDurability() => 100;

        public int DetectItemStackSize() => 1;

        public int DectectHealthBarRemaining() => 100;

        public bool DetectEmptySlot() => false;
    }
}
