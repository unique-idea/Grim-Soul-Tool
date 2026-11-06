using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace UI.Helpers
{
    public static class WindowHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        public const uint WDA_NONE = 0x00000000;
        public const uint WDA_MONITOR = 0x00000001;
        public const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        /// <summary>
        /// Instructs Windows Desktop Window Manager (DWM) to completely exclude this window
        /// from screen captures (Graphics.CopyFromScreen, BitBlt, screenshots).
        /// The window remains fully visible to the user on monitor, but any screen scanning
        /// captures what is BEHIND this window (the game screen), never the tool UI.
        /// </summary>
        public static void ExcludeFromCapture(Window window)
        {
            if (window == null) return;

            void Apply(IntPtr hwnd)
            {
                if (hwnd == IntPtr.Zero) return;
                try
                {
                    bool success = SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
                    Debug.WriteLine($"[WindowHelper] ExcludeFromCapture for '{window.GetType().Name}': {success}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[WindowHelper] Error setting affinity: {ex.Message}");
                }
            }

            var helper = new WindowInteropHelper(window);
            if (helper.Handle != IntPtr.Zero)
            {
                Apply(helper.Handle);
            }
            else
            {
                window.SourceInitialized += (s, e) =>
                {
                    IntPtr hwnd = new WindowInteropHelper(window).Handle;
                    Apply(hwnd);
                };
            }
        }

        /// <summary>
        /// Checks if any of the specified visible UI windows currently overlaps the given screen bounds.
        /// </summary>
        public static bool IsAnyWindowOverlapping(Rect screenBounds, params Window[] windows)
        {
            if (screenBounds.IsEmpty || screenBounds.Width <= 0 || screenBounds.Height <= 0)
                return false;

            foreach (var win in windows)
            {
                if (win == null || !win.IsVisible) continue;

                // Win bounds in screen DIP
                Rect winBounds = new Rect(win.Left, win.Top, Math.Max(0, win.ActualWidth), Math.Max(0, win.ActualHeight));
                if (winBounds.IntersectsWith(screenBounds))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
