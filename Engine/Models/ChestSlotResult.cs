using Engine.Models;

namespace Engine.Models
{
    public class ChestSlotResult
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int SlotIndex => Row * 5 + Col;
        public bool HasItem { get; set; }
        public Item? Item { get; set; }
        public int Quantity { get; set; } = 1;
        public string ItemDisplayName => HasItem && Item != null
            ? (string.IsNullOrEmpty(Item.DisplayName) ? Item.Name.ToString() : Item.DisplayName)
            : "Empty";

        /// <summary>
        /// Top dominant non-background hex colors scanned in this slot (up to 7 colors).
        /// </summary>
        public System.Collections.Generic.List<string> ScannedColorsHex { get; set; } = new();

        /// <summary>
        /// Ratio of non-background pixels detected in the slot.
        /// </summary>
        public double NonBgRatio { get; set; }
    }
}
