using System.ComponentModel;
using System.Windows.Media;
using Engine.Enums;

namespace UI.Models
{
    /// <summary>
    /// Represents one item entry in the whitelist panel.
    /// </summary>
    public class ItemRow : INotifyPropertyChanged
    {
        private string _itemKey = string.Empty;
        private string _description = string.Empty;
        private ItemType _type = ItemType.Resource;
        private bool _isChecked = true;
        private ImageSource? _imageSource;
        private object? _tag;

        public string ItemKey
        {
            get => _itemKey;
            set { _itemKey = value; OnPropertyChanged(nameof(ItemKey)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public ItemType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(TypeGroup));
            }
        }

        public string TypeGroup => Type switch
        {
            ItemType.Resource => "Resource",
            ItemType.Consumable => "Consumable",
            ItemType.Special => "Special",
            ItemType.Weapon => "Weapon",
            ItemType.Armor => "Armor",
            ItemType.Tool => "Tool",
            _ => "Other"
        };

        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); }
        }

        public ImageSource? ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
        }

        /// <summary>
        /// Stores underlying model object (e.g. Engine.Item).
        /// </summary>
        public object? Tag
        {
            get => _tag;
            set { _tag = value; OnPropertyChanged(nameof(Tag)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}