using Engine.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Engine.Models
{
    /// <summary>
    /// Represents a single in-game item that the bot can recognise and collect.
    /// </summary>
    public class Item
    {
        /// <summary>Unique colors associated with this item.</summary>
        public List<Color>? Colors { get; init; }
        /// <summary>Number of items in the stack.</summary>
        public int Stack { get; init; } = 1;
        /// <summary>Durability of the item.</summary>
        public int Durability { get; init; } = 100;
        /// <summary>Unique enum key identifying this item.</summary>
        public ItemName Name { get; init; }

        /// <summary>Category of this item (Resource, Food, Weapon …).</summary>
        public ItemType Type { get; init; }

        /// <summary>Human-readable label shown in the UI whitelist panel.</summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// Optional filename (without extension) of a 32×32 icon inside
        /// UI/Assets/Icons/.  Leave null to show the default placeholder.
        /// </summary>
        public string? IconKey { get; init; }
        public double ColorTolerance { get; set; } = 18.0;

        /// <summary>
        /// Whether the bot should attempt to collect this item by default.
        /// Maps to the CheckBox in the whitelist panel.
        /// </summary>
        public bool EnabledByDefault { get; init; } = true;

        /// <summary>
        /// Logic đánh giá đặc trưng riêng của từng Item khi quét rương.
        /// Trả về điểm số (>= 100 là nhận diện thành công).
        /// </summary>
        public Func<List<Color>, double>? CustomEvaluator { get; set; }
    }
}
