using Engine.Enums;
using Engine.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Engine.References.Models
{
    public static class ItemsRegistry
    {
        public static List<Item> GetDefaultItems()
        {
            return new List<Item>
            {

                new Item
                {
                    Name = ItemName.Tree,
                    Type = ItemType.Resource,
                    DisplayName = "Wood",
                    IconKey = "icon_tree",
                    EnabledByDefault = true,
                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#3B231A"),
                        ColorTranslator.FromHtml("#442B23"),
                        ColorTranslator.FromHtml("#4D342C"),
                        ColorTranslator.FromHtml("#2B1A12"),
                        ColorTranslator.FromHtml("#573E2E"),
                        ColorTranslator.FromHtml("#926958")
                    }
                },
                new Item
                {
                    Name = ItemName.PineLog,
                    Type = ItemType.Resource,
                    DisplayName = "Pine Log",
                    IconKey = "icon_pine_log",
                    EnabledByDefault = true,
                    ColorTolerance = 15.0,
                    Colors = new List<Color>
                    {
                        Color.FromArgb(139, 69, 19),
                        Color.FromArgb(160, 82, 45)
                    }
                },

                new Item
                {
                    Name = ItemName.Stone,
                    Type = ItemType.Resource,
                    DisplayName = "Stone",
                    IconKey = "icon_stone",
                    EnabledByDefault = true,
                    ColorTolerance = 12.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#7D756D"),
                        ColorTranslator.FromHtml("#696158"),
                        ColorTranslator.FromHtml("#171717"),
                        ColorTranslator.FromHtml("#8A8279"),
                        ColorTranslator.FromHtml("#736B63"),
                        ColorTranslator.FromHtml("#534B4B")
                    }
                },
                new Item
                {
                    Name = ItemName.Limestone,
                    Type = ItemType.Resource,
                    DisplayName = "Limestone",
                    IconKey = "icon_limestone",
                    EnabledByDefault = true,
                    ColorTolerance = 18.0,
                    Colors = new List<Color>
                    {
                        Color.FromArgb(192, 192, 192),
                        Color.FromArgb(211, 211, 211)
                    }
                },
                new Item
                {
                    Name = ItemName.SoulShard,
                    Type = ItemType.Resource,
                    DisplayName = "Soul Shard",
                    IconKey = "icon_soul_shard",
                    EnabledByDefault = true,
                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#646464"),
                        ColorTranslator.FromHtml("#777777"),
                        ColorTranslator.FromHtml("#505050"),
                        ColorTranslator.FromHtml("#5A5A5A"),
                        ColorTranslator.FromHtml("#6E6E6E"),
                        ColorTranslator.FromHtml("#909090")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperOre,
                    Type = ItemType.Resource,
                    DisplayName = "Copper Ore",
                    IconKey = "icon_copper_ore",
                    EnabledByDefault = true,
                    ColorTolerance = 16.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#4F2D2D"),
                        ColorTranslator.FromHtml("#402727"),
                        ColorTranslator.FromHtml("#1B1B1B"),
                        ColorTranslator.FromHtml("#663333"),
                        ColorTranslator.FromHtml("#AE644B"),
                        ColorTranslator.FromHtml("#A55A42")
                    }
                },

                new Item
                {
                    Name = ItemName.Hollyberry,
                    Type = ItemType.Consumable,
                    DisplayName = "Hollyberry",
                    IconKey = "icon_hollyberry",
                    EnabledByDefault = true,
                    ColorTolerance = 20.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#822D1C"),
                        ColorTranslator.FromHtml("#02331A"),
                        ColorTranslator.FromHtml("#E05C33"),
                        ColorTranslator.FromHtml("#02442B"),
                        ColorTranslator.FromHtml("#004840"),
                        ColorTranslator.FromHtml("#181818")
                    }
                },
                new Item
                {
                    Name = ItemName.Hawberry,
                    Type = ItemType.Consumable,
                    DisplayName = "Hawberry",
                    IconKey = "icon_hawberry",
                    EnabledByDefault = true,
                    ColorTolerance = 24.0,
                    Colors = new List<Color>
                    {

                        ColorTranslator.FromHtml("#3E601C"),
                        ColorTranslator.FromHtml("#3E4F1C"),
                        ColorTranslator.FromHtml("#30411F"),
                        ColorTranslator.FromHtml("#5B7D28"),
                        ColorTranslator.FromHtml("#476925"),

                        ColorTranslator.FromHtml("#5C233B"),
                        ColorTranslator.FromHtml("#7B2446"),
                        ColorTranslator.FromHtml("#5A1E36"),
                        ColorTranslator.FromHtml("#8C2D52"),
                        ColorTranslator.FromHtml("#A13860"),
                        ColorTranslator.FromHtml("#3A1223")
                    }
                },

                new Item
                {
                    Name = ItemName.FlaxFibber,
                    Type = ItemType.Resource,
                    DisplayName = "Flax Fiber",
                    IconKey = "icon_flax_fibber",
                    EnabledByDefault = true,
                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#634130"),
                        ColorTranslator.FromHtml("#6D4C3B"),
                        ColorTranslator.FromHtml("#191919"),
                        ColorTranslator.FromHtml("#997755"),
                        ColorTranslator.FromHtml("#CBC4B2"),
                        ColorTranslator.FromHtml("#AA8866"),
                        ColorTranslator.FromHtml("#89684F"),
                        ColorTranslator.FromHtml("#734A39"),
                        ColorTranslator.FromHtml("#6A4932")
                    }
                },
                new Item
                {
                    Name = ItemName.Cloth,
                    Type = ItemType.Resource,
                    DisplayName = "Cloth",
                    IconKey = "icon_cloth",
                    EnabledByDefault = true,
                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A87655"),
                        ColorTranslator.FromHtml("#9D6C4B"),
                        ColorTranslator.FromHtml("#1C1C1C"),
                        ColorTranslator.FromHtml("#895747"),
                        ColorTranslator.FromHtml("#78473F"),
                        ColorTranslator.FromHtml("#946342")
                    }
                },
                new Item
                {
                    Name = ItemName.ThickCloth,
                    Type = ItemType.Resource,
                    DisplayName = "Thick Cloth",
                    IconKey = "icon_thick_cloth",
                    EnabledByDefault = true,
                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#B78E6D"),
                        ColorTranslator.FromHtml("#CB9968"),
                        ColorTranslator.FromHtml("#774444"),
                        ColorTranslator.FromHtml("#8E654C"),
                        ColorTranslator.FromHtml("#B6845B"),
                        ColorTranslator.FromHtml("#7D5444")
                    }
                },
                new Item
                {
                    Name = ItemName.Rope,
                    Type = ItemType.Resource,
                    DisplayName = "Rope",
                    IconKey = "icon_rope",
                    EnabledByDefault = true,
                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#9D8664"),
                        ColorTranslator.FromHtml("#7C644E"),
                        ColorTranslator.FromHtml("#7C634C"),
                        ColorTranslator.FromHtml("#60473A"),
                        ColorTranslator.FromHtml("#B19876"),
                        ColorTranslator.FromHtml("#7B5C47"),
                        ColorTranslator.FromHtml("#AA9276")
                    }
                },

                new Item
                {
                    Name = ItemName.RawLeather,
                    Type = ItemType.Resource,
                    DisplayName = "Raw Leather",
                    IconKey = "icon_raw_leather",
                    EnabledByDefault = true,
                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#58372F"),
                        ColorTranslator.FromHtml("#997766"),
                        ColorTranslator.FromHtml("#4A3129"),
                        ColorTranslator.FromHtml("#65443B"),
                        ColorTranslator.FromHtml("#8E6D5C"),
                        ColorTranslator.FromHtml("#1B1C1C")
                    }
                },
                new Item
                {
                    Name = ItemName.Leather,
                    Type = ItemType.Resource,
                    DisplayName = "Leather",
                    IconKey = "icon_leather",
                    EnabledByDefault = true,
                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#502F1F"),
                        ColorTranslator.FromHtml("#482716"),
                        ColorTranslator.FromHtml("#361D15"),
                        ColorTranslator.FromHtml("#6D4C33"),
                        ColorTranslator.FromHtml("#795840"),
                        ColorTranslator.FromHtml("#5A3929")
                    }
                },
                new Item
                {
                    Name = ItemName.RawMeat,
                    Type = ItemType.Consumable,
                    DisplayName = "Raw Meat",
                    IconKey = "icon_raw_meat",
                    EnabledByDefault = true,
                    ColorTolerance = 20.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#71272F"),
                        ColorTranslator.FromHtml("#993344"),
                        ColorTranslator.FromHtml("#C14A5B"),
                        ColorTranslator.FromHtml("#7D2839"),
                        ColorTranslator.FromHtml("#8A2F37"),
                        ColorTranslator.FromHtml("#DDBBBB")
                    }
                },

                new Item
                {
                    Name = ItemName.LeekSeed,
                    Type = ItemType.Resource,
                    DisplayName = "Leek Seed",
                    IconKey = "icon_leek_seed",
                    EnabledByDefault = true,
                    ColorTolerance = 16.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#211921"),
                        ColorTranslator.FromHtml("#171717"),
                        ColorTranslator.FromHtml("#BFB9A8"),
                        ColorTranslator.FromHtml("#4C4C54"),
                        ColorTranslator.FromHtml("#848075"),
                        ColorTranslator.FromHtml("#B1AB9C")
                    }
                },
                new Item
                {
                    Name = ItemName.WheatSeed,
                    Type = ItemType.Resource,
                    DisplayName = "Wheat Seed",
                    IconKey = "icon_wheat_seed",
                    EnabledByDefault = true,
                    ColorTolerance = 18.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#1D1D1D"),
                        ColorTranslator.FromHtml("#BB8844"),
                        ColorTranslator.FromHtml("#131313"),
                        ColorTranslator.FromHtml("#DEAB56"),
                        ColorTranslator.FromHtml("#CB9854"),
                        ColorTranslator.FromHtml("#A4713E")
                    }
                },
                new Item
                {
                    Name = ItemName.OakSeed,
                    Type = ItemType.Resource,
                    DisplayName = "Oak Seed",
                    IconKey = "icon_oak_seed",
                    EnabledByDefault = true,
                    ColorTolerance = 17.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#8E5B28"),
                        ColorTranslator.FromHtml("#1C1C1C"),
                        ColorTranslator.FromHtml("#A76D44"),
                        ColorTranslator.FromHtml("#7F4C2A"),
                        ColorTranslator.FromHtml("#986532"),
                        ColorTranslator.FromHtml("#DFB695")
                    }
                },

                new Item
                {
                    Name = ItemName.BonePowder,
                    Type = ItemType.Special,
                    DisplayName = "Bone Powder",
                    IconKey = "icon_bone_powder",
                    EnabledByDefault = true,
                    ColorTolerance = 18.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#BB9A71"),
                        ColorTranslator.FromHtml("#996633"),
                        ColorTranslator.FromHtml("#7D8E9F"),
                        ColorTranslator.FromHtml("#D08E44"),
                        ColorTranslator.FromHtml("#B37940"),
                        ColorTranslator.FromHtml("#AD9473")
                    }
                },
                new Item
                {
                    Name = ItemName.HoneyBarrel,
                    Type = ItemType.Consumable,
                    DisplayName = "Honey Barrel",
                    IconKey = "icon_honey_barrel",
                    EnabledByDefault = true,
                    ColorTolerance = 15.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#714837"),
                        ColorTranslator.FromHtml("#61372F"),
                        ColorTranslator.FromHtml("#4C2B23"),
                        ColorTranslator.FromHtml("#4C2B33"),
                        ColorTranslator.FromHtml("#824F3E"),
                        ColorTranslator.FromHtml("#664444")
                    }
                },
                new Item
                {
                    Name = ItemName.WineFlask,
                    Type = ItemType.Consumable,
                    DisplayName = "Wine Flask",
                    IconKey = "icon_wine_flask",
                    EnabledByDefault = true,
                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {

                        ColorTranslator.FromHtml("#542B2B"),
                        ColorTranslator.FromHtml("#401F1F"),
                        ColorTranslator.FromHtml("#3B1717"),
                        ColorTranslator.FromHtml("#5F3636"),

                        ColorTranslator.FromHtml("#75443B"),
                        ColorTranslator.FromHtml("#824B3C")
                    } },
                new Item
                {
                    Name = ItemName.EmptyFlask,
                    Type = ItemType.Resource,
                    DisplayName = "Empty Flask",
                    IconKey = "icon_empty_flask",
                    EnabledByDefault = true,
                    ColorTolerance = 15.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#371616"),
                        ColorTranslator.FromHtml("#5C2B23"),
                        ColorTranslator.FromHtml("#442222"),
                        ColorTranslator.FromHtml("#281728"),
                        ColorTranslator.FromHtml("#8C4A39"),
                        ColorTranslator.FromHtml("#96543B")
                    }
                },
                new Item
                {
                    Name = ItemName.Bandage,
                    Type = ItemType.Consumable,
                    DisplayName = "Bandage",
                    IconKey = "icon_bandage",
                    EnabledByDefault = true,
                    ColorTolerance = 18.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#8A7979"),
                        ColorTranslator.FromHtml("#B79E8E"),
                        ColorTranslator.FromHtml("#7A626A"),
                        ColorTranslator.FromHtml("#A38A82"),
                        ColorTranslator.FromHtml("#8A798A"),
                        ColorTranslator.FromHtml("#7D6D75")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperShard,
                    Type = ItemType.Resource,
                    DisplayName = "Copper Scrap",
                    IconKey = "icon_copper_scrap",
                    EnabledByDefault = true,

                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {

                        ColorTranslator.FromHtml("#A65D4C"),
                        ColorTranslator.FromHtml("#965241"),
                        ColorTranslator.FromHtml("#A66653"),

                        ColorTranslator.FromHtml("#C87864"),
                        ColorTranslator.FromHtml("#B86D58"),

                        ColorTranslator.FromHtml("#6A4434"),
                        ColorTranslator.FromHtml("#462F28")
                    }
                },
                new Item
                {
                    Name = ItemName.Comb,
                    Type = ItemType.Special,
                    DisplayName = "Comb",
                    IconKey = "icon_comb",
                    EnabledByDefault = true,

                    ColorTolerance = 18.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#DEBF9E"),
                        ColorTranslator.FromHtml("#EDD9BD"),
                        ColorTranslator.FromHtml("#EED6B4"),
                        ColorTranslator.FromHtml("#E5C7A6"),
                        ColorTranslator.FromHtml("#E9D2B1"),
                        ColorTranslator.FromHtml("#F1E0C1"),
                        ColorTranslator.FromHtml("#F6ECDB")
                    }
                },
                new Item
                {
                    Name = ItemName.Mirror,
                    Type = ItemType.Special,
                    DisplayName = "Mirror",
                    IconKey = "icon_mirror",
                    EnabledByDefault = true,

                    ColorTolerance = 15.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#EFEFEF"),
                        ColorTranslator.FromHtml("#695B51"),
                        ColorTranslator.FromHtml("#977F6F"),
                        ColorTranslator.FromHtml("#E5D9CD"),
                        ColorTranslator.FromHtml("#866D6C"),
                        ColorTranslator.FromHtml("#868395")
                    }
                },
                new Item
                {
                    Name = ItemName.Skull,
                    Type = ItemType.Special,
                    DisplayName = "Skull",
                    IconKey = "icon_skull",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#7C6C5B"),
                        ColorTranslator.FromHtml("#B0A599"),
                        ColorTranslator.FromHtml("#6B5B43"),
                        ColorTranslator.FromHtml("#ABA691"),
                        ColorTranslator.FromHtml("#B7A88F"),
                        ColorTranslator.FromHtml("#51402F"),
                        ColorTranslator.FromHtml("#C9BBA5")
                    }
                },
                new Item
                {
                    Name = ItemName.Lock,
                    Type = ItemType.Resource,
                    DisplayName = "Lock",
                    IconKey = "icon_lock",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#754C3D"),
                        ColorTranslator.FromHtml("#61556B"),
                        ColorTranslator.FromHtml("#312118"),
                        ColorTranslator.FromHtml("#7F7F83"),
                        ColorTranslator.FromHtml("#5A6AA9"),
                        ColorTranslator.FromHtml("#8D9193"),
                        ColorTranslator.FromHtml("#916958")
                    }
                },
                new Item
                {
                    Name = ItemName.Cords,
                    Type = ItemType.Resource,
                    DisplayName = "Cords",
                    IconKey = "icon_cords",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#553834"),
                        ColorTranslator.FromHtml("#413647"),
                        ColorTranslator.FromHtml("#795747"),
                        ColorTranslator.FromHtml("#452D2B"),
                        ColorTranslator.FromHtml("#3A2939"),
                        ColorTranslator.FromHtml("#512C1D"),
                        ColorTranslator.FromHtml("#866657")
                    }
                },
                new Item
                {
                    Name = ItemName.IronGrate,
                    Type = ItemType.Special,
                    DisplayName = "Iron Grate",
                    IconKey = "icon_iron_grate",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#261F28"),
                        ColorTranslator.FromHtml("#5B4F57"),
                        ColorTranslator.FromHtml("#584768"),
                        ColorTranslator.FromHtml("#544753"),
                        ColorTranslator.FromHtml("#695454"),
                        ColorTranslator.FromHtml("#544354"),
                        ColorTranslator.FromHtml("#796973")
                    }
                },
                new Item
                {
                    Name = ItemName.Wire,
                    Type = ItemType.Resource,
                    DisplayName = "Wire",
                    IconKey = "icon_wire",
                    EnabledByDefault = true,

                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#312B2B"),
                        ColorTranslator.FromHtml("#604F4F"),
                        ColorTranslator.FromHtml("#6A5A59"),
                        ColorTranslator.FromHtml("#2F2727"),
                        ColorTranslator.FromHtml("#302629"),
                        ColorTranslator.FromHtml("#4F3F3F"),
                        ColorTranslator.FromHtml("#665656")
                    }
                },
                new Item
                {
                    Name = ItemName.GoldRing,
                    Type = ItemType.Special,
                    DisplayName = "Gold Ring",
                    IconKey = "icon_gold_ring",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#95643D"),
                        ColorTranslator.FromHtml("#9D6F48"),
                        ColorTranslator.FromHtml("#E9E4C3"),
                        ColorTranslator.FromHtml("#D2B88D"),
                        ColorTranslator.FromHtml("#956C43"),
                        ColorTranslator.FromHtml("#A1824C"),
                        ColorTranslator.FromHtml("#8E603A")
                    }
                },
                new Item
                {
                    Name = ItemName.TerrorScroll,
                    Type = ItemType.Special,
                    DisplayName = "Terror Scroll",
                    IconKey = "icon_terror_scroll",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#D0B78E"),
                        ColorTranslator.FromHtml("#9D7E64"),
                        ColorTranslator.FromHtml("#552544"),
                        ColorTranslator.FromHtml("#907769"),
                        ColorTranslator.FromHtml("#593749"),
                        ColorTranslator.FromHtml("#C3A787"),
                        ColorTranslator.FromHtml("#37677D")
                    }
                },
                new Item
                {
                    Name = ItemName.FarmingTools,
                    Type = ItemType.Tool,
                    DisplayName = "Farming Tools",
                    IconKey = "icon_farming_tools",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#7A7F82"),
                        ColorTranslator.FromHtml("#845134"),
                        ColorTranslator.FromHtml("#666671"),
                        ColorTranslator.FromHtml("#794B35"),
                        ColorTranslator.FromHtml("#545555"),
                        ColorTranslator.FromHtml("#94613E"),
                        ColorTranslator.FromHtml("#946443")
                    }
                },
                new Item
                {
                    Name = ItemName.WeightedMace,
                    Type = ItemType.Weapon,
                    DisplayName = "Weighted Mace",
                    IconKey = "icon_weighted_mace",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#B8816C"),
                        ColorTranslator.FromHtml("#A87E6D"),
                        ColorTranslator.FromHtml("#408AAD"),
                        ColorTranslator.FromHtml("#60685F"),
                        ColorTranslator.FromHtml("#735847"),
                        ColorTranslator.FromHtml("#946E3B"),
                        ColorTranslator.FromHtml("#544C5C")
                    }
                },

                new Item
                {
                    Name = ItemName.Bowstring,
                    Type = ItemType.Special,
                    DisplayName = "Bowstring",
                    IconKey = "icon_bowstring",
                    EnabledByDefault = true,

                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#7D6D5D"),
                        ColorTranslator.FromHtml("#563E32"),
                        ColorTranslator.FromHtml("#837458"),
                        ColorTranslator.FromHtml("#7F6C55"),
                        ColorTranslator.FromHtml("#5A4232"),
                        ColorTranslator.FromHtml("#604F3F"),
                        ColorTranslator.FromHtml("#655547")
                    }
                },

                new Item
                {
                    Name = ItemName.Candle,
                    Type = ItemType.Resource,
                    DisplayName = "Candle",
                    IconKey = "icon_candle",
                    EnabledByDefault = true,

                    ColorTolerance = 15.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#BD9260"),
                        ColorTranslator.FromHtml("#564535"),
                        ColorTranslator.FromHtml("#63371D"),
                        ColorTranslator.FromHtml("#FCD99D"),
                        ColorTranslator.FromHtml("#845A38"),
                        ColorTranslator.FromHtml("#CA9962"),
                        ColorTranslator.FromHtml("#6C482F")
                    }
                },
                new Item
                {
                    Name = ItemName.Silverware,
                    Type = ItemType.Special,
                    DisplayName = "Silverware",
                    IconKey = "icon_silverware",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#59636E"),
                        ColorTranslator.FromHtml("#8E9098"),
                        ColorTranslator.FromHtml("#969FA6"),
                        ColorTranslator.FromHtml("#666D71"),
                        ColorTranslator.FromHtml("#696A70"),
                        ColorTranslator.FromHtml("#95959B"),
                        ColorTranslator.FromHtml("#8B8F96")
                    }
                },
                new Item
                {
                    Name = ItemName.Hourglass,
                    Type = ItemType.Special,
                    DisplayName = "Hourglass",
                    IconKey = "icon_hourglass",
                    EnabledByDefault = true,

                    ColorTolerance = 16.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#703120"),
                        ColorTranslator.FromHtml("#7795A6"),
                        ColorTranslator.FromHtml("#BDBDBD"),
                        ColorTranslator.FromHtml("#B8B8A9"),
                        ColorTranslator.FromHtml("#9B6A3B"),
                        ColorTranslator.FromHtml("#9F4A34"),
                        ColorTranslator.FromHtml("#AB7E49")
                    }
                },
                new Item
                {
                    Name = ItemName.Chain,
                    Type = ItemType.Resource,
                    DisplayName = "Chain",
                    IconKey = "icon_chain",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#604F4A"),
                        ColorTranslator.FromHtml("#5C4B4B"),
                        ColorTranslator.FromHtml("#5E524A"),
                        ColorTranslator.FromHtml("#85706D"),
                        ColorTranslator.FromHtml("#1F1C1C"),
                        ColorTranslator.FromHtml("#735950"),
                        ColorTranslator.FromHtml("#9D8582")
                    }
                },
                new Item
                {
                    Name = ItemName.StoneWeapon1,
                    Type = ItemType.Weapon,
                    DisplayName = "Stone Weapon 1",
                    IconKey = "icon_stone_weapon_1",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#787070"),
                        ColorTranslator.FromHtml("#A46844"),
                        ColorTranslator.FromHtml("#8D5830"),
                        ColorTranslator.FromHtml("#2C2F3C"),
                        ColorTranslator.FromHtml("#686060"),
                        ColorTranslator.FromHtml("#8A593C"),
                        ColorTranslator.FromHtml("#696161")
                    }
                },
                new Item
                {
                    Name = ItemName.WoodenSword,
                    Type = ItemType.Weapon,
                    DisplayName = "Wooden Sword",
                    IconKey = "icon_wooden_sword",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#84634D"),
                        ColorTranslator.FromHtml("#807D9C"),
                        ColorTranslator.FromHtml("#856F72"),
                        ColorTranslator.FromHtml("#573C35"),
                        ColorTranslator.FromHtml("#4B404C"),
                        ColorTranslator.FromHtml("#474A5F"),
                        ColorTranslator.FromHtml("#53422A")
                    }
                },new Item
                {
                    Name = ItemName.Sickle,
                    Type = ItemType.Weapon,
                    DisplayName = "Sickle",
                    IconKey = "icon_sickle",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#B2A7A4"),
                        ColorTranslator.FromHtml("#A29292"),
                        ColorTranslator.FromHtml("#C9B9B9"),
                        ColorTranslator.FromHtml("#5E4138"),
                        ColorTranslator.FromHtml("#837E7B"),
                        ColorTranslator.FromHtml("#664338"),
                        ColorTranslator.FromHtml("#EBE1D4")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperTrousers,
                    Type = ItemType.Armor,
                    DisplayName = "Copper Trousers",
                    IconKey = "icon_copper_trousers",
                    EnabledByDefault = true,

                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#3B3239"),
                        ColorTranslator.FromHtml("#362E36"),
                        ColorTranslator.FromHtml("#45343D"),
                        ColorTranslator.FromHtml("#493E3F"),
                        ColorTranslator.FromHtml("#3F2F38"),
                        ColorTranslator.FromHtml("#4B333B"),
                        ColorTranslator.FromHtml("#483841")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperHauberk,
                    Type = ItemType.Armor,
                    DisplayName = "Copper Hauberk",
                    IconKey = "icon_copper_hauberk",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A15E55"),
                        ColorTranslator.FromHtml("#5D3B40"),
                        ColorTranslator.FromHtml("#5F3E45"),
                        ColorTranslator.FromHtml("#372A32"),
                        ColorTranslator.FromHtml("#452828"),
                        ColorTranslator.FromHtml("#5C3C44"),
                        ColorTranslator.FromHtml("#845253")
                    }
                },
                new Item
                {
                    Name = ItemName.LeatherHat,
                    Type = ItemType.Armor,
                    DisplayName = "Leather Hat",
                    IconKey = "icon_leather_hat",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#835959"),
                        ColorTranslator.FromHtml("#5C383A"),
                        ColorTranslator.FromHtml("#181A20"),
                        ColorTranslator.FromHtml("#4C2B2C"),
                        ColorTranslator.FromHtml("#563434"),
                        ColorTranslator.FromHtml("#583435"),
                        ColorTranslator.FromHtml("#714F47")
                    }
                },

                new Item
                {
                    Name = ItemName.LeatherGlove,
                    Type = ItemType.Armor,
                    DisplayName = "Leather Glove",
                    IconKey = "icon_leather_glove",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#8D755C"),
                        ColorTranslator.FromHtml("#533A2A"),
                        ColorTranslator.FromHtml("#31190D"),
                        ColorTranslator.FromHtml("#8A7153"),
                        ColorTranslator.FromHtml("#392317"),
                        ColorTranslator.FromHtml("#392111"),
                        ColorTranslator.FromHtml("#8E785C")
                    }
                },
                new Item
                {
                    Name = ItemName.LeatherBoots,
                    Type = ItemType.Armor,
                    DisplayName = "Leather Boots",
                    IconKey = "icon_leather_boots",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#3E2E35"),
                        ColorTranslator.FromHtml("#533D40"),
                        ColorTranslator.FromHtml("#2E252D"),
                        ColorTranslator.FromHtml("#796970"),
                        ColorTranslator.FromHtml("#3D2C34"),
                        ColorTranslator.FromHtml("#413239"),
                        ColorTranslator.FromHtml("#7A6A71")
                    }
                },
                new Item
                {
                    Name = ItemName.FibberHauberk,
                    Type = ItemType.Armor,
                    DisplayName = "Flax Hauberk",
                    IconKey = "icon_fibber_hauberk",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#D2CABD"),
                        ColorTranslator.FromHtml("#614F5B"),
                        ColorTranslator.FromHtml("#886C67"),
                        ColorTranslator.FromHtml("#9D7B78"),
                        ColorTranslator.FromHtml("#4C3C43"),
                        ColorTranslator.FromHtml("#DCCFBE"),
                        ColorTranslator.FromHtml("#B7B0A0")
                    }
                },
                new Item
                {
                    Name = ItemName.FibberTrousers,
                    Type = ItemType.Armor,
                    DisplayName = "Flax Trousers",
                    IconKey = "icon_fibber_trousers",
                    EnabledByDefault = true,

                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#2E2E3F"),
                        ColorTranslator.FromHtml("#282637"),
                        ColorTranslator.FromHtml("#6F605B"),
                        ColorTranslator.FromHtml("#574947"),
                        ColorTranslator.FromHtml("#5F5250"),
                        ColorTranslator.FromHtml("#34303D"),
                        ColorTranslator.FromHtml("#3F3741")
                    }
                },
                new Item
                {
                    Name = ItemName.WoodenWeapon1,
                    Type = ItemType.Weapon,
                    DisplayName = "Wooden Weapon 1",
                    IconKey = "icon_wooden_weapon_1",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#4C3B49"),
                        ColorTranslator.FromHtml("#6B4C3D"),
                        ColorTranslator.FromHtml("#7F5437"),
                        ColorTranslator.FromHtml("#4A3B47"),
                        ColorTranslator.FromHtml("#CE9A74"),
                        ColorTranslator.FromHtml("#8A5955"),
                        ColorTranslator.FromHtml("#A9765D")
                    }
                },
                new Item
                {
                    Name = ItemName.StoneWeapon2,
                    Type = ItemType.Weapon,
                    DisplayName = "Stone Weapon 2",
                    IconKey = "icon_stone_weapon_2",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#908682"),
                        ColorTranslator.FromHtml("#7B7375"),
                        ColorTranslator.FromHtml("#6B473F"),
                        ColorTranslator.FromHtml("#272935"),
                        ColorTranslator.FromHtml("#614A55"),
                        ColorTranslator.FromHtml("#313142"),
                        ColorTranslator.FromHtml("#69454C")
                    }
                },

                new Item
                {
                    Name = ItemName.CopperHat,
                    Type = ItemType.Armor,
                    DisplayName = "Copper Hat",
                    IconKey = "icon_copper_hat",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#835953"),
                        ColorTranslator.FromHtml("#AA796B"),
                        ColorTranslator.FromHtml("#B3796B"),
                        ColorTranslator.FromHtml("#95625F"),
                        ColorTranslator.FromHtml("#EB9981"),
                        ColorTranslator.FromHtml("#FDB194"),
                        ColorTranslator.FromHtml("#4F3636")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperGlove,
                    Type = ItemType.Armor,
                    DisplayName = "Copper Glove",
                    IconKey = "icon_copper_glove",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#705156"),
                        ColorTranslator.FromHtml("#63484B"),
                        ColorTranslator.FromHtml("#A17877"),
                        ColorTranslator.FromHtml("#5F464C"),
                        ColorTranslator.FromHtml("#4F3F47"),
                        ColorTranslator.FromHtml("#45282F"),
                        ColorTranslator.FromHtml("#4C2C30")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperBoots,
                    Type = ItemType.Armor,
                    DisplayName = "Copper Boots",
                    IconKey = "icon_copper_boots",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#422834"),
                        ColorTranslator.FromHtml("#6B434A"),
                        ColorTranslator.FromHtml("#38252E"),
                        ColorTranslator.FromHtml("#32262E"),
                        ColorTranslator.FromHtml("#885056"),
                        ColorTranslator.FromHtml("#32212A"),
                        ColorTranslator.FromHtml("#422831")
                    }
                },
                new Item
                {
                    Name = ItemName.LeatherHauberk,
                    Type = ItemType.Armor,
                    DisplayName = "Leather Hauberk",
                    IconKey = "icon_leather_hauberk",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#594339"),
                        ColorTranslator.FromHtml("#5B4439"),
                        ColorTranslator.FromHtml("#BB7E68"),
                        ColorTranslator.FromHtml("#643C38"),
                        ColorTranslator.FromHtml("#8F5948"),
                        ColorTranslator.FromHtml("#9B6455"),
                        ColorTranslator.FromHtml("#271821")
                    }
                },
                new Item
                {
                    Name = ItemName.MapFragment,
                    Type = ItemType.Special,
                    DisplayName = "Map Fragment",
                    IconKey = "icon_map_fragment",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#B29E81"),
                        ColorTranslator.FromHtml("#6C4F33"),
                        ColorTranslator.FromHtml("#D0C7B7"),
                        ColorTranslator.FromHtml("#82725F"),
                        ColorTranslator.FromHtml("#644C34"),
                        ColorTranslator.FromHtml("#745852"),
                        ColorTranslator.FromHtml("#5E4D3C")
                    }
                },
                new Item
                {
                    Name = ItemName.MorningStar,
                    Type = ItemType.Armor,
                    DisplayName = "Morning Star",
                    IconKey = "icon_morning_star",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#835137"),
                        ColorTranslator.FromHtml("#614E53"),
                        ColorTranslator.FromHtml("#5D4D6F"),
                        ColorTranslator.FromHtml("#393248"),
                        ColorTranslator.FromHtml("#403133"),
                        ColorTranslator.FromHtml("#745852"),
                        ColorTranslator.FromHtml("#FCDCC5")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperWeapon1,
                    Type = ItemType.Weapon,
                    DisplayName = "Vũ khí đồng 1",
                    IconKey = "icon_copper_sword",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#7F5858"),
                        ColorTranslator.FromHtml("#846665"),
                        ColorTranslator.FromHtml("#EFBDB7"),
                        ColorTranslator.FromHtml("#372A32"),
                        ColorTranslator.FromHtml("#D9BAB1"),
                        ColorTranslator.FromHtml("#826067"),
                        ColorTranslator.FromHtml("#A27981")
                    }
                },
                new Item
                {
                    Name = ItemName.Wheel,
                    Type = ItemType.Special,
                    DisplayName = "Wooden Wheel",
                    IconKey = "icon_wheel",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#745345"),
                        ColorTranslator.FromHtml("#2C3940"),
                        ColorTranslator.FromHtml("#3A738D"),
                        ColorTranslator.FromHtml("#51352F"),
                        ColorTranslator.FromHtml("#6E4E43"),
                        ColorTranslator.FromHtml("#885B52"),
                        ColorTranslator.FromHtml("#513831")
                    }
                },
                new Item
                {
                    Name = ItemName.Tin,
                    Type = ItemType.Resource,
                    DisplayName = "Tin",
                    IconKey = "icon_tin",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#4B4B53"),
                        ColorTranslator.FromHtml("#9B9B95"),
                        ColorTranslator.FromHtml("#4D4D5B"),
                        ColorTranslator.FromHtml("#C6C6B5"),
                        ColorTranslator.FromHtml("#BBBBAD"),
                        ColorTranslator.FromHtml("#6A6A75"),
                        ColorTranslator.FromHtml("#E1E1CD")
                    }
                },
                new Item
                {
                    Name = ItemName.EmptyVial,
                    Type = ItemType.Special,
                    DisplayName = "Empty Vial",
                    IconKey = "icon_empty_vial",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#6E757E"),
                        ColorTranslator.FromHtml("#8A9494"),
                        ColorTranslator.FromHtml("#2E4B4E"),
                        ColorTranslator.FromHtml("#445564"),
                        ColorTranslator.FromHtml("#4B565E"),
                        ColorTranslator.FromHtml("#84909C"),
                        ColorTranslator.FromHtml("#364754")
                    }
                },
                new Item
                {
                    Name = ItemName.MandrakeTincture,
                    Type = ItemType.Special,
                    DisplayName = "Mandrake Tincture",
                    IconKey = "icon_mandrake_tincture",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#CB9978"),
                        ColorTranslator.FromHtml("#919CA7"),
                        ColorTranslator.FromHtml("#E4D1A2"),
                        ColorTranslator.FromHtml("#673D2B"),
                        ColorTranslator.FromHtml("#EEE4C4"),
                        ColorTranslator.FromHtml("#8D93AD"),
                        ColorTranslator.FromHtml("#E9DAB7")
                    }
                },

                new Item
                {
                    Name = ItemName.SpikeMace,
                    Type = ItemType.Armor,
                    DisplayName = "Spike Mace",
                    IconKey = "icon_spike_mace",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#9A5E45"),
                        ColorTranslator.FromHtml("#8F5C3A"),
                        ColorTranslator.FromHtml("#6C4C55"),
                        ColorTranslator.FromHtml("#756057"),
                        ColorTranslator.FromHtml("#514051"),
                        ColorTranslator.FromHtml("#A46A46"),
                        ColorTranslator.FromHtml("#B37751")
                    }
                },
                new Item
                {
                    Name = ItemName.CopperWeapon2,
                    Type = ItemType.Weapon,
                    DisplayName = "Copper Weapon 2",
                    IconKey = "icon_copper_weapon_2",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A98075"),
                        ColorTranslator.FromHtml("#8C5C59"),
                        ColorTranslator.FromHtml("#F2BAA5"),
                        ColorTranslator.FromHtml("#4B3135"),
                        ColorTranslator.FromHtml("#A97A76"),
                        ColorTranslator.FromHtml("#35262B"),
                        ColorTranslator.FromHtml("#7C5355")
                    }
                },
                new Item
                {
                    Name = ItemName.FlangedMace,
                    Type = ItemType.Weapon,
                    DisplayName = "Flanged Mace",
                    IconKey = "icon_flanged_mace",
                    EnabledByDefault = true,

                    ColorTolerance = 14.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#A9897D"),
                        ColorTranslator.FromHtml("#795858"),
                        ColorTranslator.FromHtml("#916464"),
                        ColorTranslator.FromHtml("#7B627B"),
                        ColorTranslator.FromHtml("#665063"),
                        ColorTranslator.FromHtml("#655251"),
                        ColorTranslator.FromHtml("#7D5D61")
                    }
                },

                new Item
                {
                    Name = ItemName.LeatherTrousers,
                    Type = ItemType.Armor,
                    DisplayName = "Leather Trousers",
                    IconKey = "icon_leather_trousers",
                    EnabledByDefault = true,

                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#121214"),
                        ColorTranslator.FromHtml("#24242B"),
                        ColorTranslator.FromHtml("#453B3C"),
                        ColorTranslator.FromHtml("#2F272F"),
                        ColorTranslator.FromHtml("#2B252B"),
                        ColorTranslator.FromHtml("#2A2930"),
                        ColorTranslator.FromHtml("#292629")
                    }
                },
                new Item
                {
                    Name = ItemName.FibberHat,
                    Type = ItemType.Armor,
                    DisplayName = "Flax Hat",
                    IconKey = "icon_fibber_hat",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#967B57"),
                        ColorTranslator.FromHtml("#241C28"),
                        ColorTranslator.FromHtml("#3E3335"),
                        ColorTranslator.FromHtml("#795A4A"),
                        ColorTranslator.FromHtml("#C7AC75"),
                        ColorTranslator.FromHtml("#664D3B"),
                        ColorTranslator.FromHtml("#A07E5A")
                    }
                },
                new Item
                {
                    Name = ItemName.FibberGlove,
                    Type = ItemType.Armor,
                    DisplayName = "Flax Boots",
                    IconKey = "icon_fibber_glove",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#B09E81"),
                        ColorTranslator.FromHtml("#5F5148"),
                        ColorTranslator.FromHtml("#B09B7C"),
                        ColorTranslator.FromHtml("#3F3D3D"),
                        ColorTranslator.FromHtml("#C7B493"),
                        ColorTranslator.FromHtml("#61564A"),
                        ColorTranslator.FromHtml("#6C5B50")
                    }
                },
                new Item
                {
                    Name = ItemName.FibberBoots,
                    Type = ItemType.Armor,
                    DisplayName = "Flax Glove",
                    IconKey = "icon_fibber_boots",
                    EnabledByDefault = true,

                    ColorTolerance = 13.5,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#45393B"),
                        ColorTranslator.FromHtml("#9B826A"),
                        ColorTranslator.FromHtml("#A68B70"),
                        ColorTranslator.FromHtml("#A98F72"),
                        ColorTranslator.FromHtml("#836959"),
                        ColorTranslator.FromHtml("#241B17"),
                        ColorTranslator.FromHtml("#A78A72")
                    }
                }
                ,
                new Item
                {
                    Name = ItemName.Fibula,
                    Type = ItemType.Special,
                    DisplayName = "Fibula",
                    IconKey = "icon_fibula",
                    EnabledByDefault = true,
                    ColorTolerance = 13.0,
                    Colors = new List<Color>
                    {
                        ColorTranslator.FromHtml("#D5B988"),
                        ColorTranslator.FromHtml("#936432"),
                        ColorTranslator.FromHtml("#694227"),
                        ColorTranslator.FromHtml("#916C41"),
                        ColorTranslator.FromHtml("#9A6E46"),
                        ColorTranslator.FromHtml("#2B2A2A"),
                        ColorTranslator.FromHtml("#CFB37A")
                    }
                }
            };
        }
    }
}
