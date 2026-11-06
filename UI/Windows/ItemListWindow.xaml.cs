using Engine.Enums;
using Engine.Models;
using Engine.References.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UI.Helpers;
using UI.Models;
using UI.Services;

namespace UI.Windows
{
    public partial class ItemListWindow : Window
    {
        private readonly ObservableCollection<ItemRow> _items = new();
        private readonly CollectionViewSource _viewSource = new();
        private string _currentFilter = "All";
        private bool _isInitialLoading = false;

        private static readonly SolidColorBrush ActiveBtnBg = new(Color.FromArgb(0x35, 0x00, 0xE6, 0x76));
        private static readonly SolidColorBrush ActiveBtnBorder = new(Color.FromArgb(0x90, 0x00, 0xE6, 0x76));
        private static readonly SolidColorBrush ActiveBtnFg = new(Color.FromArgb(0xFF, 0x00, 0xE6, 0x76));

        private static readonly SolidColorBrush InactiveBtnBg = new(Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF));
        private static readonly SolidColorBrush InactiveBtnBorder = new(Color.FromArgb(0x25, 0xFF, 0xFF, 0xFF));
        private static readonly SolidColorBrush InactiveBtnFg = new(Color.FromArgb(0xB0, 0xFF, 0xFF, 0xFF));

        /// <summary>
        /// True if user has manually moved this window, preventing toolbar auto-dock override.
        /// </summary>
        public bool IsCustomPositioned { get; set; } = false;

        // 1. Constructor máº·c Ä‘á»‹nh: Tá»± Ä‘á»™ng load danh sÃ¡ch tá»« Engine
        public ItemListWindow()
        {
            InitializeComponent();
            SetupViewSource();

            LocationChanged += (s, e) =>
            {
                if (IsLoaded && IsMouseOver)
                    IsCustomPositioned = true;
            };

            // Load danh sÃ¡ch Item tá»« Engine
            LoadEngineItems(ItemsRegistry.GetDefaultItems());
            UpdateFilterButtonStyles();
        }

        // 2. Overload Constructor: Nháº­n danh sÃ¡ch Item truyá»n tá»« ngoÃ i vÃ o (náº¿u cÃ³)
        public ItemListWindow(IEnumerable<Item> engineItems)
        {
            InitializeComponent();
            SetupViewSource();

            LoadEngineItems(engineItems);
            UpdateFilterButtonStyles();
        }

        private void SetupViewSource()
        {
            _viewSource.Source = _items;
            _viewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ItemRow.TypeGroup)));
            _viewSource.Filter += ViewSource_Filter;
            ItemsCtrl.ItemsSource = _viewSource.View;
        }

        private void ViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not ItemRow row) return;

            if (_currentFilter == "All")
            {
                e.Accepted = true;
                return;
            }

            if (Enum.TryParse<ItemType>(_currentFilter, out var filterType))
            {
                e.Accepted = (row.Type == filterType);
            }
            else
            {
                e.Accepted = true;
            }
        }

        // â”€â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Náº¡p danh sÃ¡ch cÃ¡c class Item tá»« Engine vÃ o UI vÃ  khÃ´i phá»¥c cÃ i Ä‘áº·t Ä‘Ã£ lÆ°u.
        /// </summary>
        public void LoadEngineItems(IEnumerable<Item> engineItems)
        {
            _isInitialLoading = true;
            try
            {
                _items.Clear();

                var config = ConfigService.Load();
                var savedSettings = config?.ItemSettings;

                foreach (var item in engineItems)
                {
                    ImageSource? icon = ImageHelper.LoadIcon(item.IconKey);

                    string key = item.Name.ToString();
                    bool isChecked = item.EnabledByDefault;

                    if (savedSettings != null && savedSettings.TryGetValue(key, out bool savedState))
                    {
                        isChecked = savedState;
                    }

                    var row = new ItemRow
                    {
                        ItemKey = key,
                        Description = item.DisplayName,
                        Type = item.Type,
                        IsChecked = isChecked,
                        ImageSource = icon,
                        Tag = item
                    };

                    row.PropertyChanged += Row_PropertyChanged;
                    _items.Add(row);
                }
            }
            finally
            {
                _isInitialLoading = false;
            }
        }

        private void Row_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInitialLoading) return;
            if (e.PropertyName == nameof(ItemRow.IsChecked))
            {
                SaveSettings();
            }
        }

        /// <summary>
        /// LÆ°u cÃ i Ä‘áº·t báº­t/táº¯t (checked/unchecked) cá»§a cÃ¡c items vÃ o config.json
        /// </summary>
        public void SaveSettings()
        {
            if (_isInitialLoading) return;
            var settings = GetItemSettings();
            ConfigService.SaveItemSettings(settings);
        }

        public Dictionary<string, bool> GetItemSettings()
        {
            var dict = new Dictionary<string, bool>();
            foreach (var row in _items)
            {
                if (!string.IsNullOrEmpty(row.ItemKey))
                {
                    dict[row.ItemKey] = row.IsChecked;
                }
                else if (row.Tag is Item item)
                {
                    dict[item.Name.ToString()] = row.IsChecked;
                }
            }
            return dict;
        }

        /// <summary>
        /// Láº¥y ra danh sÃ¡ch cÃ¡c Item mÃ  ngÆ°á»i dÃ¹ng Ä‘Ã£ tick chá»n trÃªn UI Ä‘á»ƒ gá»­i ngÆ°á»£c láº¡i cho Engine bot cháº¡y.
        /// </summary>
        public List<Item> GetSelectedItems()
        {
            return _items
                .Where(row => row.IsChecked && row.Tag is Item)
                .Select(row => (Item)row.Tag!)
                .ToList();
        }

        public void AddRow(string description, ImageSource? imageSource = null)
        {
            var row = new ItemRow
            {
                Description = description,
                IsChecked = true,
                ImageSource = imageSource
            };
            row.PropertyChanged += Row_PropertyChanged;
            _items.Add(row);
        }

        public void ClearRows() => _items.Clear();

        public IReadOnlyList<ItemRow> GetRows() => _items;

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                IsCustomPositioned = true;
                DragMove();
            }
        }

        private void Row_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Náº¿u ngÆ°á»i dÃ¹ng click trá»±c tiáº¿p vÃ o CheckBox thÃ¬ CheckBox tá»± xá»­ lÃ½
            if (e.OriginalSource is CheckBox || (e.OriginalSource is DependencyObject dep && FindVisualParent<CheckBox>(dep) != null))
            {
                return;
            }

            if (sender is FrameworkElement fe && fe.DataContext is ItemRow row)
            {
                row.IsChecked = !row.IsChecked;
                e.Handled = true;
            }
        }

        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindVisualParent<T>(parentObject);
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                _currentFilter = tag;
                UpdateFilterButtonStyles();
                _viewSource.View.Refresh();
            }
        }

        private void UpdateFilterButtonStyles()
        {
            SetBtnStyle(BtnFilterAll, _currentFilter == "All");
            SetBtnStyle(BtnFilterResource, _currentFilter == "Resource");
            SetBtnStyle(BtnFilterConsumable, _currentFilter == "Consumable");
            SetBtnStyle(BtnFilterSpecial, _currentFilter == "Special");
        }

        private static void SetBtnStyle(Button btn, bool isActive)
        {
            if (btn == null) return;
            btn.Background = isActive ? ActiveBtnBg : InactiveBtnBg;
            btn.BorderBrush = isActive ? ActiveBtnBorder : InactiveBtnBorder;
            btn.Foreground = isActive ? ActiveBtnFg : InactiveBtnFg;
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _viewSource.View)
            {
                if (item is ItemRow row)
                {
                    row.IsChecked = true;
                }
            }
            SaveSettings();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                if (Owner is ToolbarWindow tb)
                {
                    tb.PauseScanning();
                }
                e.Handled = true;
            }
        }

        private void BtnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _viewSource.View)
            {
                if (item is ItemRow row)
                {
                    row.IsChecked = false;
                }
            }
            SaveSettings();
        }
    }
}