using Engine.Enums;
using Engine.Models;
using Engine.References.Models;
using Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.Services.Implements
{
    public class MoveStep : IMoveStep
    {
        // ─── Win32 API mô phỏng bấm/đè bàn phím ──────────────────────────────
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_KEYDOWN = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        // Mã Virtual Key cho bàn phím
        private const byte VK_W = 0x57;
        private const byte VK_A = 0x41;
        private const byte VK_S = 0x53;
        private const byte VK_D = 0x44;

        /// <summary>
        /// Thời gian đè phím cho mỗi bước di chuyển (mới tính theo millisecond)
        /// </summary>
        private int MoveDuration = 300; // ms per grid unit

        /// <summary>
        /// Di chuyển từ viền khung đỏ (Player Center Zone) tới vị trí Mục tiêu
        /// </summary>
        /// <param name="targetCoordinate">Tọa độ tâm vật phẩm (CenterRow, CenterCol)</param>
        /// <param name="gridN">Độ mịn lưới hiện tại (10, 20, 30...)</param>
        /// <param name="centerIgnoredRadius">Bán kính khung đỏ (1 đối với 10x10, 2 đối với 20x20...)</param>
        public void MoveFromCenterZone(Coordinate targetCoordinate, int gridN, int centerIgnoredRadius)
        {
            int centerBase = gridN / 2;

            // Xác định phạm vi biên của khung đỏ
            int minCenterRow = centerBase - centerIgnoredRadius;
            int maxCenterRow = centerBase + centerIgnoredRadius - 1;
            int minCenterCol = centerBase - centerIgnoredRadius;
            int maxCenterCol = centerBase + centerIgnoredRadius - 1;

            // Tính độ lệch hàng (deltaRow) từ viền khung đỏ gần nhất
            int deltaRow = 0;
            if (targetCoordinate.Row > maxCenterRow)
                deltaRow = targetCoordinate.Row - maxCenterRow;      // Đi Xuống (S)
            else if (targetCoordinate.Row < minCenterRow)
                deltaRow = targetCoordinate.Row - minCenterRow;      // Đi Lên (W)

            // Tính độ lệch cột (deltaCol) từ viền khung đỏ gần nhất
            int deltaCol = 0;
            if (targetCoordinate.Column > maxCenterCol)
                deltaCol = targetCoordinate.Column - maxCenterCol;  // Đi Phải (D)
            else if (targetCoordinate.Column < minCenterCol)
                deltaCol = targetCoordinate.Column - minCenterCol;  // Đi Trái (A)

            // Tiến hành bấm phím dựa trên khoảng cách thực tế từ viền
            ExecuteStepMovement(deltaRow, deltaCol);
        }

        private void ExecuteStepMovement(int deltaRow, int deltaCol)
        {
            int stepsRow = Math.Abs(deltaRow);
            int stepsCol = Math.Abs(deltaCol);

            // 1. Đi các bước chéo trước
            int diagonalSteps = Math.Min(stepsRow, stepsCol);
            for (int i = 0; i < diagonalSteps; i++)
            {
                if (deltaRow < 0 && deltaCol > 0) UpRight();        // WA / WD
                else if (deltaRow < 0 && deltaCol < 0) UpLeft();
                else if (deltaRow > 0 && deltaCol > 0) DownRight();
                else if (deltaRow > 0 && deltaCol < 0) DownLeft();
            }

            // 2. Đi nốt các bước thẳng còn lại
            int remainingRowSteps = stepsRow - diagonalSteps;
            int remainingColSteps = stepsCol - diagonalSteps;

            for (int i = 0; i < remainingRowSteps; i++)
            {
                if (deltaRow < 0) Up(); else Down();
            }

            for (int i = 0; i < remainingColSteps; i++)
            {
                if (deltaCol < 0) Left(); else Right();
            }
        }

        // ─── Các hàm đè phím theo hướng cụ thể ──────────────────────────────

        private void Up() => PressKeys(new byte[] { VK_W });
        private void Down() => PressKeys(new byte[] { VK_S });
        private void Left() => PressKeys(new byte[] { VK_A });
        private void Right() => PressKeys(new byte[] { VK_D });

        private void UpLeft() => PressKeys(new byte[] { VK_W, VK_A });
        private void UpRight() => PressKeys(new byte[] { VK_W, VK_D });
        private void DownLeft() => PressKeys(new byte[] { VK_S, VK_A });
        private void DownRight() => PressKeys(new byte[] { VK_S, VK_D });

        /// <summary>
        /// Hàm cốt lõi: Nhấn đè danh sách các phím cùng lúc trong khoảng thời gian MoveDuration
        /// </summary>
        private void PressKeys(byte[] keys)
        {
            // 1. Key Down (Giữ phím)
            foreach (var key in keys)
            {
                keybd_event(key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            }

            // 2. Chờ đúng thời gian MoveDuration (Mô phỏng khoảng thời gian di chuyển 1 ô)
            Thread.Sleep(MoveDuration);

            // 3. Key Up (Nhả phím)
            foreach (var key in keys)
            {
                keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }

            // Nghỉ ngắn giữa các bước để game kịp ghi nhận
            Thread.Sleep(50);
        }
    }
}