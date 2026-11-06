using Engine.Enums;
using Engine.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Engine.References.Models
{
    /// <summary>
    /// Registry chứa danh sách màu mẫu các đối tượng / tài nguyên xuất hiện trên Mini-map.
    /// </summary>
    public static class MapObjectRegistry
    {
        public static List<Item> GetDefaultMapObjects()
        {
            return new List<Item>
            {
                // 1. Icon Quả Mọng (Berry Icon trên Mini-map/UI)
                new Item
                {
                    Name = ItemName.Berry,
                    Type = ItemType.Resource,
                    DisplayName = "Quả Mọng (Berry)",
                    IconKey = "icon_berry",
                    EnabledByDefault = true,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#BCAF82"), // Sand
                        ColorTranslator.FromHtml("#AB996F"), // Camel
                        ColorTranslator.FromHtml("#C8816A"), // Burnt Peach
                        ColorTranslator.FromHtml("#B3A376"), // Sand 2
                        ColorTranslator.FromHtml("#CD7E68"), // Burnt Peach 2
                        ColorTranslator.FromHtml("#C98E70"), // Toasted Almond
                        ColorTranslator.FromHtml("#C0B286")  // Sand 3
                    }
                },

                // 2. Icon Quặng Đồng (Copper Ore Icon trên Mini-map/UI)
                new Item
                {
                    Name = ItemName.CopperOre,
                    Type = ItemType.Resource,
                    DisplayName = "Quặng Đồng (Copper Ore)",
                    IconKey = "icon_copper_ore",
                    EnabledByDefault = true,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#EA783A"), // Atomic Tangerine
                        ColorTranslator.FromHtml("#B0AEA7"), // Silver
                        ColorTranslator.FromHtml("#DB6837"), // Spicy Paprika
                        ColorTranslator.FromHtml("#E46E37"), // Atomic Tangerine 2
                        ColorTranslator.FromHtml("#9B9082"), // Grey Olive
                        ColorTranslator.FromHtml("#9D9487"), // Grey Olive 2
                        ColorTranslator.FromHtml("#E47B41")  // Atomic Tangerine 3
                    }
                },

                // 3. Cây Thông trên Mini-map (Pine Tree Icon)
                new Item
                {
                    Name = ItemName.Tree,
                    Type = ItemType.Resource,
                    DisplayName = "Cây Thông (Pine Tree)",
                    IconKey = "icon_tree",
                    EnabledByDefault = true,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A07254"), // Faded Copper
                        ColorTranslator.FromHtml("#A77B5C"), // Faded Copper 2
                        ColorTranslator.FromHtml("#A7795A"), // Faded Copper 3
                        ColorTranslator.FromHtml("#AE876A"), // Dusty Taupe
                        ColorTranslator.FromHtml("#997155"), // Faded Copper 4
                        ColorTranslator.FromHtml("#83684E"), // Olive Wood
                        ColorTranslator.FromHtml("#A57959")  // Faded Copper 5
                    }
                },

                // 4. Cây Lanh (Flax Fiber Icon)
                new Item
                {
                    Name = ItemName.FlaxFibber,
                    Type = ItemType.Resource,
                    DisplayName = "Cây Lanh (Flax Fiber)",
                    IconKey = "icon_flax_fibber",
                    EnabledByDefault = true,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#B6A77B"), // Sand
                        ColorTranslator.FromHtml("#AE9B6F"), // Sand 2
                        ColorTranslator.FromHtml("#A79B76"), // Sand 3
                        ColorTranslator.FromHtml("#B5A67A"), // Sand 4
                        ColorTranslator.FromHtml("#B0A176"), // Sand 5
                        ColorTranslator.FromHtml("#C2B688"), // Sand 6
                        ColorTranslator.FromHtml("#A59773")  // Khaki Beige
                    }
                },

                // 5. Icon Quặng Đá trên Mini-map (Rock / Limestone Node Icon)
                new Item
                {
                    Name = ItemName.Stone,
                    Type = ItemType.Resource,
                    DisplayName = "Quặng Đá (Rock / Limestone)",
                    IconKey = "icon_stone",
                    EnabledByDefault = true,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A1988D"), // Grey Olive
                        ColorTranslator.FromHtml("#9F9589"), // Grey Olive 2
                        ColorTranslator.FromHtml("#9F9E97"), // Grey Olive 3
                        ColorTranslator.FromHtml("#B3B3AF"), // Silver
                        ColorTranslator.FromHtml("#AFAEA7"), // Silver 2
                        ColorTranslator.FromHtml("#9F978A"), // Grey Olive 4
                        ColorTranslator.FromHtml("#9B9082")  // Grey Olive 5
                    }
                }
            };
        }
    }
}
