using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.Helpers;

namespace UI.Windows
{
    /// <summary>
    /// Interaction logic for TerminalLogWindow.xaml
    /// Provides real-time log viewing for each scan pipeline (R1..R6 + All).
    /// </summary>
    public partial class TerminalLogWindow : Window
    {
        private int _currentTab = 1; // Default to R1 Healthbar
        private int _totalLogs = 0;

        /// <summary>
        /// True if user has manually moved the window, preventing auto-dock override.
        /// </summary>
        public bool IsCustomPositioned { get; set; } = false;

        // Pipeline theme accent colors
        private static readonly Color ColorAll = Color.FromRgb(0, 230, 118);       // Green
        private static readonly Color ColorR1 = Color.FromRgb(0, 229, 255);        // Cyan (Healthbar)
        private static readonly Color ColorR2 = Color.FromRgb(255, 145, 0);        // Orange (Mini Map)
        private static readonly Color ColorR3 = Color.FromRgb(224, 64, 251);       // Magenta (Inventory)
        private static readonly Color ColorR4 = Color.FromRgb(255, 215, 0);        // Gold (Equipment)
        private static readonly Color ColorR5 = Color.FromRgb(255, 23, 68);         // Red (Enemy Bar)
        private static readonly Color ColorR6 = Color.FromRgb(0, 230, 118);        // Emerald Green (Chest)

        private static readonly SolidColorBrush BrushInactiveBg = new(Color.FromArgb(22, 255, 255, 255));
        private static readonly SolidColorBrush BrushInactiveFg = new(Color.FromRgb(160, 160, 160));

        public TerminalLogWindow()
        {
            InitializeComponent();
            SelectTab(1); // Default to R1 Healthbar

            // Exclude from desktop screen captures so scanning never captures this window
            WindowHelper.ExcludeFromCapture(this);

            LocationChanged += (s, e) =>
            {
                // If user drags or moves the window
                if (IsLoaded && IsMouseOver)
                {
                    IsCustomPositioned = true;
                }
            };
        }

        // ─── Window Drag & Close ──────────────────────────────────────────
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                IsCustomPositioned = true;
                DragMove();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && !(e.OriginalSource is TextBox or Button or CheckBox))
            {
                IsCustomPositioned = true;
                DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        // ─── Tab Switching ────────────────────────────────────────────────
        private void BtnTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tagStr && int.TryParse(tagStr, out int tabIndex))
            {
                SelectTab(tabIndex);
            }
        }

        public void SelectTab(int tabIndex)
        {
            _currentTab = tabIndex;

            // Reset all tab button styles
            ResetTabStyle(BtnTabAll);
            ResetTabStyle(BtnTabR1);
            ResetTabStyle(BtnTabR2);
            ResetTabStyle(BtnTabR3);
            ResetTabStyle(BtnTabR4);
            ResetTabStyle(BtnTabR5);
            ResetTabStyle(BtnTabR6);

            // Hide all textboxes
            TxtLogAll.Visibility = Visibility.Collapsed;
            TxtLogR1.Visibility = Visibility.Collapsed;
            TxtLogR2.Visibility = Visibility.Collapsed;
            TxtLogR3.Visibility = Visibility.Collapsed;
            TxtLogR4.Visibility = Visibility.Collapsed;
            TxtLogR5.Visibility = Visibility.Collapsed;
            TxtLogR6.Visibility = Visibility.Collapsed;

            // Highlight selected tab & show its textbox
            switch (tabIndex)
            {
                case 0:
                    HighlightTab(BtnTabAll, ColorAll);
                    TxtLogAll.Visibility = Visibility.Visible;
                    TxtStatus.Text = "Stream: All";
                    break;
                case 1:
                    HighlightTab(BtnTabR1, ColorR1);
                    TxtLogR1.Visibility = Visibility.Visible;
                    TxtStatus.Text = "Stream: Health";
                    break;
                case 2:
                    HighlightTab(BtnTabR2, ColorR2);
                    TxtLogR2.Visibility = Visibility.Visible;
                    TxtStatus.Text = "Stream: Map";
                    break;
                case 3:
                    HighlightTab(BtnTabR3, ColorR3);
                    TxtLogR3.Visibility = Visibility.Visible;
                    TxtStatus.Text = "Stream: Inventory";
                    break;
                case 4:
                    HighlightTab(BtnTabR4, ColorR4);
                    TxtLogR4.Visibility = Visibility.Visible;
                    TxtStatus.Text = "Stream: Equipment";
                    break;
                case 5:
                    HighlightTab(BtnTabR5, ColorR5);
                    TxtLogR5.Visibility = Visibility.Visible;
                    TxtStatus.Text = "Stream: Enemy";
                    break;
                case 6:
                    HighlightTab(BtnTabR6, ColorR6);
                    TxtLogR6.Visibility = Visibility.Visible;
                    TxtStatus.Text = "Stream: Chest";
                    break;
            }
        }

        private static void ResetTabStyle(Button btn)
        {
            btn.Background = BrushInactiveBg;
            btn.Foreground = BrushInactiveFg;
            btn.BorderBrush = new SolidColorBrush(Color.FromArgb(37, 255, 255, 255));
        }

        private static void HighlightTab(Button btn, Color accent)
        {
            btn.Background = new SolidColorBrush(Color.FromArgb(45, accent.R, accent.G, accent.B));
            btn.Foreground = new SolidColorBrush(accent);
            btn.BorderBrush = new SolidColorBrush(Color.FromArgb(160, accent.R, accent.G, accent.B));
        }

        // ─── Logging API ──────────────────────────────────────────────────
        /// <summary>
        /// Appends a real-time message to the specified pipeline tab (and to All tab).
        /// Pipeline IDs: 1: Healthbar, 2: Mini Map, 3: Inventory, 4: Equipment, 5: Enemy Bar, 6: Chest, 0: All / General.
        /// </summary>
        public void Log(int pipelineId, string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => Log(pipelineId, message));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            // For specific pipeline tab: show only timestamp + message (no repeated pipeline name tag)
            string specificEntry = $"[{timestamp}] {message}" + Environment.NewLine;

            // For All tab: short tag prefix so user knows which pipeline produced it
            string tag = pipelineId switch
            {
                1 => "[Health]",
                2 => "[Map]",
                3 => "[Inventory]",
                4 => "[Equipment]",
                5 => "[Enemy]",
                6 => "[Chest]",
                _ => "[MR]"
            };
            string allEntry = $"[{timestamp}] {tag} {message}" + Environment.NewLine;

            // 1. Append to specific tab box
            TextBox? targetBox = pipelineId switch
            {
                1 => TxtLogR1,
                2 => TxtLogR2,
                3 => TxtLogR3,
                4 => TxtLogR4,
                5 => TxtLogR5,
                6 => TxtLogR6,
                _ => null
            };

            if (targetBox != null)
            {
                AppendAndTrim(targetBox, specificEntry);
            }

            // 2. Always append to All box
            AppendAndTrim(TxtLogAll, allEntry);

            _totalLogs++;
            TxtLogCount.Text = $"{_totalLogs} entries";
        }

        private void AppendAndTrim(TextBox box, string text)
        {
            box.AppendText(text);

            // Cap buffer at ~50 rows to keep UI clean and prevent overload
            const int maxRows = 50;
            if (box.LineCount > maxRows)
            {
                int excessLines = box.LineCount - maxRows;
                int cutIndex = box.GetCharacterIndexFromLineIndex(excessLines);
                if (cutIndex > 0)
                {
                    box.Text = box.Text.Substring(cutIndex);
                }
            }

            if (ChkAutoScroll.IsChecked == true)
            {
                box.ScrollToEnd();
            }
        }

        // ─── Clear & Copy ─────────────────────────────────────────────────
        private TextBox? GetCurrentTextBox()
        {
            return _currentTab switch
            {
                0 => TxtLogAll,
                1 => TxtLogR1,
                2 => TxtLogR2,
                3 => TxtLogR3,
                4 => TxtLogR4,
                5 => TxtLogR5,
                6 => TxtLogR6,
                _ => TxtLogAll
            };
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var box = GetCurrentTextBox();
            if (box != null)
            {
                box.Clear();
            }
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            var box = GetCurrentTextBox();
            if (box != null && !string.IsNullOrEmpty(box.Text))
            {
                Clipboard.SetText(box.Text);
            }
        }
    }
}
