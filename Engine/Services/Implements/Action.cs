using Engine.Enums;
using Engine.Models;
using Engine.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.Services.Implements
{
    public class Action : IAction
    {
        // ─── Win32 API ───────────────────────────────────────────────────────
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private const uint KEYEVENTF_KEYDOWN = 0x0000; // Win32 standard key down is 0
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private const byte VK_F = 0x46;
        private const byte VK_H = 0x48; // Phím H (Quick Heal)
        private const byte VK_A = 0x41; // Phím A 
        private const byte VK_P = 0x50; // Phím P (Auto Farm)
        private const byte VK_C = 0x43; // Phím C (Sneak / Crouch)
        private const byte VK_X = 0x58; // Phím X (Close Chest / Loot)
        private const byte VK_R = 0x52; // Phim R (Remove item)
        private const byte VK_O = 0x4F; // Phim O (Confirm Yes)
        private const byte VK_Y = 0x59; // Phim Y (Confirm Yes)
        private const byte VK_1 = 0x31;

        private int KeyPressDuration = 300;

        // ─── THIẾT LẬP CHỐNG OVER-HEAL (ATOMIC & NON-BLOCKING) ─────────────────
        // 0 = Rảnh (Ready), 1 = Đang trong quá trình Heal/Cooldown (Busy)
        private static long _healEndTimeUtcTicks = 0;
        private static int _isExecutingHeal = 0; // 0 = Idle, 1 = Pressing Key
        private static int _isExecutingFarm = 0;
        private static int _isHoldingF = 0;

        public bool IsHoldingAttack => Volatile.Read(ref _isHoldingF) == 1;

        // Cooldown thực tế trong Grim Soul, mặc định 1000ms
        public static int HealCooldownMs { get; set; } = 1000;

        /// <summary>
        /// Thời gian còn lại (ms) trong quá trình countdown cooldown hồi máu.
        /// </summary>
        public static int RemainingCooldownMs
        {
            get
            {
                long endTicks = Volatile.Read(ref _healEndTimeUtcTicks);
                if (endTicks == 0) return 0;
                long nowTicks = DateTime.UtcNow.Ticks;
                if (nowTicks >= endTicks) return 0;
                return (int)((endTicks - nowTicks) / TimeSpan.TicksPerMillisecond);
            }
        }


        int IAction.HealCooldownMs
        {
            get => HealCooldownMs;
            set => HealCooldownMs = value;
        }



        public static bool BlockC { get; set; } = false;
        public static bool BlockAutoFarm { get; set; } = false;

        public void Attack()
        {
            PressAttack();
        }

        public void StartHoldingAttack()
        {



            if (Interlocked.CompareExchange(ref _isHoldingF, 1, 0) == 0)
            {
                byte scan = (byte)MapVirtualKey(VK_F, 0);
                keybd_event(VK_F, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Holding F (Attack)...");
            }
        }

        public void StopHoldingAttack()
        {
            if (Interlocked.CompareExchange(ref _isHoldingF, 0, 1) == 1)
            {
                byte scan = (byte)MapVirtualKey(VK_F, 0);
                keybd_event(VK_F, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Released F (Attack stopped).");
            }
        }

        public void PressAttack()
        {


            ExecutePressAttack();
        }




        private void ExecutePressAttack()
        {
            byte scan = (byte)MapVirtualKey(VK_F, 0);
            keybd_event(VK_F, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(120);
            keybd_event(VK_F, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Pressed F once (Attack/Loot).");
        }

        public void Crouch() => PressC();

        public void PressC() => PressC(false);

        public void PressC(bool ignoreBlock = false)
        {
            if (BlockC && !ignoreBlock)
            {
                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Key C blocked.");
                return;
            }

            byte scan = (byte)MapVirtualKey(VK_C, 0);
            keybd_event(VK_C, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(120);
            keybd_event(VK_C, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Pressed C.");
        }

        public void CloseChest() => PressX();

        public void PressX()
        {
            byte scan = (byte)MapVirtualKey(VK_X, 0);
            keybd_event(VK_X, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(120);
            keybd_event(VK_X, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Pressed X (Close Chest/Loot).");
        }

        public void ClickAt(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(40);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(60);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Clicked at ({x}, {y}).");
        }

        public void DoubleClickAt(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(40);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(70);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Double-clicked at ({x}, {y}).");
        }

        public void PressR()
        {
            byte scan = (byte)MapVirtualKey(VK_R, 0);
            keybd_event(VK_R, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(200);
            keybd_event(VK_R, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Pressed R (Remove).");
        }

        public void PressO()
        {
            byte scan = (byte)MapVirtualKey(VK_O, 0);
            keybd_event(VK_O, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(200);
            keybd_event(VK_O, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Pressed O (Confirm).");
        }

        public void PressY() => PressO();

        public void AutoFarm()
        {
            // Đảm bảo tại một thời điểm chỉ có 1 lệnh bấm phím P được gửi đi
            if (BlockAutoFarm) return;
            if (Interlocked.CompareExchange(ref _isExecutingFarm, 1, 0) != 0)
                return;

            try
            {
                byte scan = (byte)MapVirtualKey(VK_P, 0);

                // Gửi sự kiện nhấn phím P
                keybd_event(VK_P, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                Thread.Sleep(KeyPressDuration > 0 ? KeyPressDuration : 120);

                // Gửi sự kiện nhả phím P
                keybd_event(VK_P, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);

                // Đệm 100ms để cửa sổ game/giản lập ghi nhận nhịp bấm phím hoàn chỉnh
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Action AutoFarm Error]: {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange(ref _isExecutingFarm, 0);
            }
        }

        public void OpenChest()
        {
            int OpenChestDelay = 5000;
            byte scan = (byte)MapVirtualKey(VK_F, 0);
            keybd_event(VK_F, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(KeyPressDuration);
            keybd_event(VK_F, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(OpenChestDelay);
        }

        public void PlaceAllItems()
        {
            throw new NotImplementedException();
        }

        public void ChangeWeapon()
        {
            throw new NotImplementedException();
        }

        public void CollectItem()
        {
            byte scan = (byte)MapVirtualKey(VK_F, 0);
            keybd_event(VK_F, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Thread.Sleep(KeyPressDuration);
            keybd_event(VK_F, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            Thread.Sleep(100);
        }

        public void InteractItem()
        {
            const int totalHits = 4;
            const int ActionDelay = 500;
            byte scan = (byte)MapVirtualKey(VK_F, 0);

            for (int i = 0; i < totalHits; i++)
            {
                keybd_event(VK_F, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                Thread.Sleep(KeyPressDuration);
                keybd_event(VK_F, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if (i < totalHits - 1)
                {
                    Thread.Sleep(ActionDelay);
                }
            }

            Thread.Sleep(200);
        }

        public void CraftItem(Item item)
        {
            throw new NotImplementedException();
        }

        public void DoubleClickItem(Input input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Thực thi phím H để hồi máu (Quick Heal).
        /// Sử dụng Atomic Ticks Lock để loại bỏ hoàn toàn lỗi Double Click và Over-heal.
        /// </summary>

        public bool Heal()
        {
            long nowTicks = DateTime.UtcNow.Ticks;
            long endTicks = Volatile.Read(ref _healEndTimeUtcTicks);

            // 1. Kiểm tra Cooldown Ticks: Nếu còn trong thời gian chờ 2.2s -> Bỏ qua lập tức
            if (nowTicks < endTicks)
            {
                return false;
            }

            // 2. Atomic Lock: Đảm bảo chỉ duy nhất 1 Thread được bấm phím tại một thời điểm
            if (Interlocked.CompareExchange(ref _isExecutingHeal, 1, 0) != 0)
            {
                return false;
            }

            int cooldown = HealCooldownMs > 0 ? HealCooldownMs : 1000;
            // Cập nhật ngay lập tức mốc Ticks kết thúc mới trước khi bấm phím
            Volatile.Write(ref _healEndTimeUtcTicks, DateTime.UtcNow.Ticks + (cooldown * TimeSpan.TicksPerMillisecond));

            try
            {
                byte scan = (byte)MapVirtualKey(VK_H, 0);

                // Gửi sự kiện KeyDown
                keybd_event(VK_H, scan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                Thread.Sleep(KeyPressDuration);

                // Gửi sự kiện KeyUp
                keybd_event(VK_H, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);

                // Bouncing Guard: Đệm 150ms để giả lập và game ghi nhận nhịp bấm phím đơn chuẩn xác
                Thread.Sleep(150);

                Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Action] Pressed H -> Quick Heal triggered ({cooldown}ms CD).");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Action Heal Error]: {ex.Message}");
                // Nếu lỗi Win32, reset lại Ticks để cho phép bấm lại
                Volatile.Write(ref _healEndTimeUtcTicks, 0);
                return false;
            }
            finally
            {
                // Nhả cờ thực thi phím H
                Interlocked.Exchange(ref _isExecutingHeal, 0);
            }
        }

        public void InitInventory()
        {
            throw new NotImplementedException();
        }

        public void FinishFarmingTurn()
        {
            throw new NotImplementedException();
        }
    }
}
