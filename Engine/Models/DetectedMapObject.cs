using Engine.Enums;
using System.Drawing;

namespace Engine.Models
{
    /// <summary>
    /// Đối tượng / Tài nguyên được phát hiện trên Mini-map.
    /// </summary>
    public class DetectedMapObject
    {
        public ItemName Name { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public Point Center { get; set; }
        public double DistanceFromCenter { get; set; } // Khoảng cách (pixels) từ tâm nhân vật
        public double DeltaX { get; set; } // Độ lệch X so với tâm (âm = Trái, dương = Phải)
        public double DeltaY { get; set; } // Độ lệch Y so với tâm (âm = Trên, dương = Dưới)
        public int PixelMatchCount { get; set; }
    }
}
